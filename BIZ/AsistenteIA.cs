using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using BIZ.Datos;
using BIZ.Modelo;

namespace BIZ
{
    /*
       ASISTENTE CON IA

       La idea: el modelo NO entra a la base. Nosotros consultamos la base con lo que ya
       tenemos (TurnoDatos, MedicoDatos), armamos un resumen en texto con los datos que
       necesita ESA persona, y se lo pasamos al modelo junto con la pregunta. El modelo
       solo redacta la respuesta. Asi no puede inventar datos ni ver lo que no le toca:
       un paciente solo recibe la disponibilidad y SUS turnos; un medico, SU agenda.

       Que puede preguntar cada uno:
         - Administradores (Seguridad, Usuarios, Especialistas, Turnos, Reportes):
           estadisticas de turnos, ej. que dia de la semana se piden mas.
         - Medico: sus pacientes del dia, a que hora, y que horarios le quedan libres.
         - Paciente: que dias hay lugar segun la especialidad, y sacar el turno ahi mismo.
    */
    public class AsistenteIA
    {
        // Los mismos 7 horarios que usa el almanaque de PortalPaciente
        public static readonly string[] Horarios = { "09:00", "10:00", "11:00", "12:00", "15:00", "16:00", "17:00" };

        // Cuantos dias para adelante mira el asistente cuando busca lugar
        private const int DiasAdelante = 14;

        private static readonly CultureInfo Ar = new CultureInfo("es-AR");

        // Lo que vuelve al navegador
        public class Respuesta
        {
            public string Texto { get; set; }
            public Propuesta Propuesta { get; set; }   // solo pacientes: el turno que se le ofrece sacar
        }

        public class Propuesta
        {
            public string Especialidad { get; set; }
            public string Fecha { get; set; }       // yyyy-MM-dd
            public string FechaTexto { get; set; }  // "martes 28 de octubre"
            public string Hora { get; set; }
            public int IdMedico { get; set; }       // 0 = sin preferencia
            public string Medico { get; set; }
        }

        public static Respuesta Preguntar(int idUsuario, string rol, string pregunta, List<Dictionary<string, string>> historial)
        {
            string hoy = DateTime.Now.ToString("dddd d 'de' MMMM 'de' yyyy, HH:mm", Ar);

            string sistema;
            if (rol == Roles.Paciente) sistema = PromptPaciente(idUsuario);
            else if (rol == Roles.Medico) sistema = PromptMedico(idUsuario);
            else sistema = PromptAdministrador();

            sistema = "Sos el asistente del sistema de turnos de un consultorio médico. Hoy es " + hoy + ".\n"
                    + "Respondé siempre en español rioplatense, corto y claro. Usá SOLO los datos de abajo: "
                    + "si algo no está en los datos, decí que no tenés esa información. No inventes números, nombres ni horarios.\n\n"
                    + sistema;

            var mensajes = new List<Dictionary<string, string>>();
            mensajes.Add(Mensaje("system", sistema));
            mensajes.AddRange(historial);
            mensajes.Add(Mensaje("user", pregunta));

            if (rol != Roles.Paciente)
                return new Respuesta { Texto = Ollama.Chat(mensajes, null) };

            // Al paciente lo obligamos a contestar con un JSON fijo, para saber si quiere sacar un turno
            string crudo = Ollama.Chat(mensajes, EsquemaPaciente());
            var datos = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(crudo);

            Respuesta r = new Respuesta { Texto = Convert.ToString(datos["respuesta"]) };
            if (datos.ContainsKey("reservar") && datos["reservar"] is bool && (bool)datos["reservar"])
            {
                r.Propuesta = ValidarPropuesta(Convert.ToString(datos["especialidad"]), Convert.ToString(datos["fecha"]),
                                               Convert.ToString(datos["hora"]), Convert.ToString(datos["medico"]));
                if (r.Propuesta == null)
                    r.Texto += "\n\n(Ese horario ya no está disponible. Pedime otro día u hora.)";
            }
            return r;
        }

        public static Dictionary<string, string> Mensaje(string rol, string texto)
        {
            return new Dictionary<string, string> { { "role", rol }, { "content", texto } };
        }

        /* ----- Administradores --------------------------------------------- */

        private static string PromptAdministrador()
        {
            List<Turno> turnos = TurnoDatos.ListarTodos();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Le respondés a un administrador del consultorio. Total de turnos en el sistema: " + turnos.Count + ".");

            // Ranking por dia de la semana del turno, ya ordenado de mayor a menor
            sb.AppendLine("\nTURNOS SOLICITADOS POR DÍA DE LA SEMANA (todos los estados, de mayor a menor):");
            foreach (var g in turnos.GroupBy(t => t.Fecha.DayOfWeek).OrderByDescending(g => g.Count()))
                sb.AppendLine("- " + Ar.DateTimeFormat.GetDayName(g.Key) + ": " + g.Count()
                    + " (confirmados " + g.Count(t => t.Estado == "Confirmado")
                    + ", pendientes " + g.Count(t => t.Estado == "Solicitado")
                    + ", rechazados " + g.Count(t => t.Estado == "Rechazado") + ")");
            foreach (DayOfWeek d in Enum.GetValues(typeof(DayOfWeek)))
                if (!turnos.Any(t => t.Fecha.DayOfWeek == d))
                    sb.AppendLine("- " + Ar.DateTimeFormat.GetDayName(d) + ": 0");

            sb.AppendLine("\nTURNOS POR ESPECIALIDAD (de mayor a menor):");
            foreach (var g in turnos.GroupBy(t => t.Especialidad).OrderByDescending(g => g.Count()))
                sb.AppendLine("- " + g.Key + ": " + g.Count());

            sb.AppendLine("\nTURNOS POR ESTADO:");
            foreach (var g in turnos.GroupBy(t => t.Estado).OrderByDescending(g => g.Count()))
                sb.AppendLine("- " + g.Key + ": " + g.Count());

            sb.AppendLine("\nTURNOS POR HORARIO (de mayor a menor):");
            foreach (var g in turnos.GroupBy(t => t.Hora).OrderByDescending(g => g.Count()).Take(7))
                sb.AppendLine("- " + g.Key + " hs: " + g.Count());

            sb.AppendLine("\nTURNOS POR MÉDICO (sin contar los que todavía no tienen médico):");
            foreach (var g in turnos.Where(t => t.IdMedico.HasValue).GroupBy(t => t.NombreMedico).OrderByDescending(g => g.Count()))
                sb.AppendLine("- " + g.Key + ": " + g.Count());

            sb.AppendLine("\nTURNOS POR MES:");
            foreach (var g in turnos.GroupBy(t => new DateTime(t.Fecha.Year, t.Fecha.Month, 1)).OrderBy(g => g.Key))
                sb.AppendLine("- " + g.Key.ToString("MMMM yyyy", Ar) + ": " + g.Count());

            List<Turno> proximos = turnos.Where(t => t.Fecha.Date >= DateTime.Today && t.Fecha.Date < DateTime.Today.AddDays(7)
                                                     && t.Estado != "Rechazado").ToList();
            sb.AppendLine("\nPRÓXIMOS 7 DÍAS: " + proximos.Count + " turnos (" + proximos.Count(t => t.Estado == "Confirmado")
                + " confirmados, " + proximos.Count(t => t.Estado == "Solicitado") + " sin confirmar).");
            sb.AppendLine("HOY: " + turnos.Count(t => t.Fecha.Date == DateTime.Today && t.Estado != "Rechazado") + " turnos.");
            sb.AppendLine("PENDIENTES DE CONFIRMAR (todas las fechas): " + turnos.Count(t => t.Estado == "Solicitado") + ".");

            sb.AppendLine("\nSi te preguntan qué día se piden más turnos, contestá con el primero del ranking por día de la semana y su cantidad, y mencioná el segundo (si hay empate, nombrá todos los empatados).");
            return sb.ToString();
        }

        /* ----- Medicos ------------------------------------------------------ */

        private static string PromptMedico(int idUsuario)
        {
            Medico medico = MedicoDatos.ObtenerPorUsuario(idUsuario);
            if (medico == null)
                return "El usuario no está vinculado a ninguna ficha de médico. Decile que le avise al administrador.";

            List<Turno> agenda = TurnoDatos.ListarTodosPorMedico(medico.IdMedico).Where(t => t.Estado != "Rechazado").ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Le respondés al Dr/a. " + medico.NombreCompleto + " (" + medico.Especialidad + "). Ayudalo a organizar su día.");
            sb.AppendLine("Sus horarios de atención son todos los días a las " + string.Join(", ", Horarios) + " (un paciente por horario).");

            List<Turno> hoy = agenda.Where(t => t.Fecha.Date == DateTime.Today).OrderBy(t => t.Hora).ToList();
            sb.AppendLine("\nPACIENTES DE HOY: " + hoy.Count(t => t.Estado == "Confirmado") + " confirmados"
                + (hoy.Any(t => t.Estado == "Solicitado") ? " y " + hoy.Count(t => t.Estado == "Solicitado") + " solicitudes sin confirmar" : "") + ".");
            foreach (Turno t in hoy)
                sb.AppendLine("- " + t.Hora + " hs: " + t.NombrePaciente + " · " + t.TipoConsulta + " · " + (t.Estado == "Confirmado" ? "confirmado" : "sin confirmar"));

            List<string> ocupadas = hoy.Where(t => t.Estado == "Confirmado").Select(t => t.Hora).ToList();
            string[] libres = Horarios.Where(h => !ocupadas.Contains(h)).ToArray();
            sb.AppendLine("Horarios libres hoy: " + (libres.Length == 0 ? "ninguno" : string.Join(", ", libres)) + ".");
            if (hoy.Count > 0)
                sb.AppendLine("Primer paciente: " + hoy.First().Hora + " hs. Último: " + hoy.Last().Hora + " hs.");

            List<Turno> semana = agenda.Where(t => t.Fecha.Date > DateTime.Today && t.Fecha.Date <= DateTime.Today.AddDays(7)
                                                   && t.Estado == "Confirmado").OrderBy(t => t.Fecha).ThenBy(t => t.Hora).ToList();
            sb.AppendLine("\nPRÓXIMOS 7 DÍAS (sin contar hoy): " + semana.Count + " turnos confirmados en total.");
            for (int i = 1; i <= 7; i++)
            {
                DateTime dia = DateTime.Today.AddDays(i);
                List<Turno> delDia = semana.Where(t => t.Fecha.Date == dia).ToList();
                sb.AppendLine("- " + dia.ToString("dddd d 'de' MMMM", Ar) + ": "
                    + (delDia.Count == 0 ? "sin turnos" : delDia.Count + (delDia.Count == 1 ? " paciente a las " : " pacientes, a las ")
                       + string.Join(", ", delDia.Select(t => t.Hora + " (" + t.NombrePaciente + ")"))));
            }

            sb.AppendLine("\nSOLICITUDES ESPERANDO QUE LAS ACEPTE O RECHACE: " + agenda.Count(t => t.Estado == "Solicitado" && t.Fecha.Date >= DateTime.Today) + ".");
            return sb.ToString();
        }

        /* ----- Pacientes ---------------------------------------------------- */

        private static string PromptPaciente(int idUsuario)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Le respondés a un PACIENTE. Lo ayudás a saber qué días hay turno según la especialidad y a sacar un turno.");
            sb.AppendLine("Se atiende todos los días a las " + string.Join(", ", Horarios) + ".");

            sb.AppendLine("\nDISPONIBILIDAD DE LOS PRÓXIMOS " + DiasAdelante + " DÍAS (fecha: horarios libres):");
            foreach (string esp in MedicoDatos.ListarEspecialidades())
            {
                List<Medico> medicos = MedicoDatos.ListarPorEspecialidad(esp);
                sb.AppendLine("\n" + esp.ToUpper() + " — médicos: " + string.Join("; ", medicos.Select(m => m.NombreCompleto)));
                Dictionary<DateTime, List<string>> libres = Libres(medicos.Select(m => m.IdMedico).ToList());
                foreach (var dia in libres)
                    sb.AppendLine("- " + dia.Key.ToString("yyyy-MM-dd") + " (" + dia.Key.ToString("dddd d 'de' MMMM", Ar) + "): "
                        + (dia.Value.Count == 0 ? "COMPLETO" : string.Join(", ", dia.Value)));
            }

            List<Turno> mios = TurnoDatos.ListarPorPaciente(idUsuario).Where(t => t.Fecha.Date >= DateTime.Today && t.Estado != "Rechazado").ToList();
            sb.AppendLine("\nTURNOS QUE YA TIENE ESTE PACIENTE:");
            if (mios.Count == 0) sb.AppendLine("- ninguno");
            foreach (Turno t in mios)
                sb.AppendLine("- " + t.Fecha.ToString("dddd d 'de' MMMM", Ar) + " " + t.Hora + " hs, " + t.Especialidad
                    + (t.IdMedico.HasValue ? " con " + t.NombreMedico : "") + " (" + (t.Estado == "Confirmado" ? "confirmado" : "esperando confirmación") + ")");

            sb.AppendLine(@"
CÓMO CONTESTAR (formato JSON):
- ""respuesta"": tu respuesta para el paciente. Si pregunta qué días hay, listá uno por renglón los próximos días con lugar (hasta 7), con día de la semana, fecha y los horarios libres; no respondas solo ""todos los días"". Si un día está COMPLETO, no lo ofrezcas.
- Si el paciente quiere SACAR un turno y queda claro el día y la hora (o pidió ""lo antes posible"": elegí el primer horario libre de esa especialidad), poné ""reservar"": true y completá ""especialidad"" (escrita igual que arriba), ""fecha"" (yyyy-MM-dd) y ""hora"" (HH:mm), y en ""respuesta"" decile qué turno le proponés y que lo confirme con el botón.
- Si nombró a un médico, poné su apellido en ""medico""; si no, dejalo vacío.
- Si falta la especialidad, preguntásela y poné ""reservar"": false. Si solo pregunta qué días hay, ""reservar"": false.
- Nunca propongas un horario que no figure como libre arriba.");
            return sb.ToString();
        }

        private static object EsquemaPaciente()
        {
            var texto = new Dictionary<string, object> { { "type", "string" } };
            return new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "respuesta", texto },
                        { "reservar", new Dictionary<string, object> { { "type", "boolean" } } },
                        { "especialidad", texto },
                        { "fecha", texto },
                        { "hora", texto },
                        { "medico", texto }
                    }
                },
                { "required", new[] { "respuesta", "reservar", "especialidad", "fecha", "hora", "medico" } }
            };
        }

        // Horarios libres por dia, desde hoy y por DiasAdelante dias, sumando a todos los medicos que se pasen.
        // Igual que el almanaque: un horario esta ocupado si el medico tiene un turno CONFIRMADO a esa hora.
        private static Dictionary<DateTime, List<string>> Libres(List<int> medicos)
        {
            var resultado = new Dictionary<DateTime, List<string>>();
            for (int i = 0; i < DiasAdelante; i++)
                resultado[DateTime.Today.AddDays(i)] = new List<string>();

            DateTime ultimo = DateTime.Today.AddDays(DiasAdelante - 1);
            foreach (int idMedico in medicos)
                for (DateTime mes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); mes <= ultimo; mes = mes.AddMonths(1))
                    foreach (DiaAgenda d in TurnoDatos.AgendaMes(idMedico, mes.Year, mes.Month))
                    {
                        if (!resultado.ContainsKey(d.Fecha.Date)) continue;
                        foreach (string h in Horarios)
                            if (!d.Horarios.Contains(h) && !resultado[d.Fecha.Date].Contains(h) && !YaPaso(d.Fecha, h))
                                resultado[d.Fecha.Date].Add(h);
                    }

            foreach (var lista in resultado.Values)
                lista.Sort(string.CompareOrdinal);
            return resultado;
        }

        private static bool YaPaso(DateTime fecha, string hora)
        {
            return fecha.Date == DateTime.Today && string.CompareOrdinal(hora, DateTime.Now.ToString("HH:mm")) <= 0;
        }

        // Revisa contra la base que el turno que propuso el modelo exista y siga libre.
        // Devuelve null si no sirve. Se usa al proponer y otra vez al confirmar.
        public static Propuesta ValidarPropuesta(string especialidad, string fecha, string hora, string medico)
        {
            // El modelo a veces escribe "9:00" en vez de "09:00"
            hora = (hora ?? "").Trim();
            if (hora.Length == 4) hora = "0" + hora;

            DateTime dia;
            if (!DateTime.TryParseExact((fecha ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dia)
                || dia < DateTime.Today || dia > DateTime.Today.AddDays(DiasAdelante - 1) || !Horarios.Contains(hora))
                return null;

            // En la base las especialidades van sin tilde ("Cardiologia") y el modelo suele ponerla
            string esp = MedicoDatos.ListarEspecialidades()
                .FirstOrDefault(e => string.Compare(e, (especialidad ?? "").Trim(), Ar, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0);
            if (esp == null) return null;

            List<Medico> medicos = MedicoDatos.ListarPorEspecialidad(esp);

            // Si nombro a un medico y ese medico tiene la hora libre, va con el; si no, "sin preferencia"
            Medico elegido = string.IsNullOrWhiteSpace(medico) ? null : medicos.FirstOrDefault(m =>
                m.NombreCompleto.IndexOf(medico.Trim(), StringComparison.OrdinalIgnoreCase) >= 0
                || medico.IndexOf(m.Apellido, StringComparison.OrdinalIgnoreCase) >= 0);

            if (elegido != null && Libres(new List<int> { elegido.IdMedico })[dia].Contains(hora))
                return Armar(esp, dia, hora, elegido);
            if (Libres(medicos.Select(m => m.IdMedico).ToList())[dia].Contains(hora))
                return Armar(esp, dia, hora, null);
            return null;
        }

        private static Propuesta Armar(string especialidad, DateTime dia, string hora, Medico medico)
        {
            return new Propuesta
            {
                Especialidad = especialidad,
                Fecha = dia.ToString("yyyy-MM-dd"),
                FechaTexto = dia.ToString("dddd d 'de' MMMM", Ar),
                Hora = hora,
                IdMedico = medico == null ? 0 : medico.IdMedico,
                Medico = medico == null ? "Sin preferencia (lo asigna el consultorio)" : medico.NombreCompleto
            };
        }
    }
}
