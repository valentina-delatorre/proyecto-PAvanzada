using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BIZ.Datos;
using BIZ.Modelo;

/*
   INICIO

   La pantalla a la que llega Seguridad (y cualquiera que no tenga un modulo propio).
   Funciona como la planilla de recepcion: que pasa hoy, la agenda de la semana,
   que pedidos esperan confirmacion, que dia se piden mas turnos y como esta cada
   profesional. Todo sale de la base al momento de abrir la pagina.

   Los links a otros modulos solo aparecen si la persona tiene ese modulo habilitado
   (los mismos permisos que usa el menu y Global.asax).
*/

namespace Consultorio2026
{
    public partial class Inicio : System.Web.UI.Page
    {
        private static readonly CultureInfo Ar = new CultureInfo("es-AR");

        // Un renglon de turno, ya listo para mostrar
        public class Fila
        {
            public string Fecha { get; set; }
            public string Hora { get; set; }
            public string Paciente { get; set; }
            public string Especialidad { get; set; }
            public string Medico { get; set; }
            public string Estado { get; set; }
        }

        // Un dia de la agenda con sus turnos
        public class Dia
        {
            public string Titulo { get; set; }
            public string Fecha { get; set; }
            public List<Fila> Turnos { get; set; }
            public bool Vacio { get { return Turnos.Count == 0; } }
        }

        private Dictionary<int, string> nombres;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack || Session["IdUsuario"] == null)
                return;

            int idUsuario = (int)Session["IdUsuario"];
            DateTime hoy = DateTime.Today;
            nombres = UsuarioDatos.NombresReales();

            List<Turno> turnos = TurnoDatos.ListarTodos();
            List<Turno> vigentes = turnos.Where(t => t.Estado != "Rechazado").ToList();
            List<Turno> deHoy = vigentes.Where(t => t.Fecha.Date == hoy).OrderBy(t => t.Hora).ToList();
            List<Turno> pendientes = turnos.Where(t => t.Estado == "Solicitado" && t.Fecha.Date >= hoy)
                                           .OrderBy(t => t.Fecha).ThenBy(t => t.Hora).ToList();
            int semana = vigentes.Count(t => t.Fecha.Date >= hoy && t.Fecha.Date < hoy.AddDays(7));

            // ----- Cabecera -----
            string nombre;
            nombres.TryGetValue(idUsuario, out nombre);
            LtFecha.Text = hoy.ToString("dddd d 'de' MMMM", Ar);
            LtSaludo.Text = Saludo() + ", " + (nombre ?? Session["NombreUsuario"].ToString()).Split(' ')[0] + ".";
            LtResumen.Text = Resumen(deHoy, pendientes.Count, vigentes);

            LtHoy.Text = deHoy.Count.ToString();
            LtSemana.Text = semana.ToString();
            LtPorConfirmar.Text = pendientes.Count.ToString();
            if (pendientes.Count == 0)
                DivPorConfirmar.Attributes["class"] = "";   // sin pedidos no hace falta resaltarlo

            // ----- Agenda: hoy y los 6 dias que siguen -----
            List<Dia> agenda = new List<Dia>();
            for (int i = 0; i < 7; i++)
            {
                DateTime dia = hoy.AddDays(i);
                agenda.Add(new Dia
                {
                    Titulo = i == 0 ? "Hoy" : i == 1 ? "Mañana" : Mayuscula(dia.ToString("dddd", Ar)),
                    Fecha = dia.ToString("d 'de' MMMM", Ar),
                    Turnos = vigentes.Where(t => t.Fecha.Date == dia).OrderBy(t => t.Hora).Select(Armar).ToList()
                });
            }
            RpAgenda.DataSource = agenda;
            RpAgenda.DataBind();

            // ----- Pedidos sin confirmar -----
            RpPendientes.DataSource = pendientes.Take(5).Select(Armar);
            RpPendientes.DataBind();
            LbSinPendientes.Visible = pendientes.Count == 0;

            // ----- Demanda por dia de la semana (de lunes a domingo) -----
            DayOfWeek[] orden = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
                                  DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
            var porDia = orden.Select(d => new { Dia = d, Cantidad = turnos.Count(t => t.Fecha.DayOfWeek == d) }).ToList();
            int maximo = Math.Max(1, porDia.Max(d => d.Cantidad));
            RpDemanda.DataSource = porDia.Select(d => new
            {
                Dia = Mayuscula(Ar.DateTimeFormat.GetAbbreviatedDayName(d.Dia)).TrimEnd('.'),
                d.Cantidad,
                Altura = Math.Max(3, d.Cantidad * 100 / maximo),   // un minimo para que se vea la barra en cero
                EsMaximo = d.Cantidad == maximo && d.Cantidad > 0
            });
            RpDemanda.DataBind();

            List<string> masPedidos = porDia.Where(d => d.Cantidad == maximo).Select(d => Ar.DateTimeFormat.GetDayName(d.Dia)).ToList();
            LtDemanda.Text = turnos.Count == 0
                ? "Todavía no hay turnos cargados."
                : Mayuscula(string.Join(" y ", masPedidos)) + (masPedidos.Count > 1 ? " son los días" : " es el día")
                  + " con más turnos pedidos: " + maximo + " de " + turnos.Count + ".";

            // ----- Profesionales y su semana -----
            RpProfesionales.DataSource = MedicoDatos.ListarTodos().Where(m => m.Activo).OrderBy(m => m.Especialidad).Select(m =>
            {
                int carga = vigentes.Count(t => t.IdMedico == m.IdMedico && t.Fecha.Date >= hoy && t.Fecha.Date < hoy.AddDays(7));
                return new
                {
                    Iniciales = m.Nombre.Substring(0, 1) + m.Apellido.Substring(0, 1),
                    Nombre = m.Nombre + " " + m.Apellido,
                    m.Especialidad,
                    Semana = carga == 0 ? "Sin turnos" : carga + (carga == 1 ? " turno" : " turnos")
                };
            });
            RpProfesionales.DataBind();

            // ----- Usuarios -----
            List<UsuarioListado> usuarios = UsuarioDatos.ListarTodos();
            int inactivos = usuarios.Count(u => !u.Activo);
            LtUsuarios.Text = usuarios.Count + " usuarios" + (inactivos > 0 ? " <span>· " + inactivos + " dado" + (inactivos == 1 ? "" : "s") + " de baja</span>" : "");
            LtRoles.Text = string.Join(" · ", usuarios.GroupBy(u => u.Rol).OrderByDescending(g => g.Count())
                                                     .Select(g => g.Count() + " " + Rol(g.Key, g.Count())));

            // ----- Links: solo a los modulos que tiene -----
            List<string> modulos = AccesoDatos.ModulosHabilitados(idUsuario);
            HlTurnos.Visible = HlRevisar.Visible = modulos.Contains("Gestion de Turnos");
            HlEspecialistas.Visible = modulos.Contains("ABM Especialistas");
            HlAccesos.Visible = modulos.Contains(AccesoDatos.ModuloAccesos);
        }

        // La frase de la cabecera: "Hoy hay 3 turnos, de 09:00 a 16:00. Hay 2 pedidos esperando confirmación."
        private static string Resumen(List<Turno> deHoy, int pendientes, List<Turno> vigentes)
        {
            string texto;
            if (deHoy.Count == 0)
            {
                Turno proximo = vigentes.Where(t => t.Fecha.Date > DateTime.Today).OrderBy(t => t.Fecha).ThenBy(t => t.Hora).FirstOrDefault();
                texto = "Hoy no hay turnos agendados."
                      + (proximo == null ? "" : " El próximo es el " + proximo.Fecha.ToString("dddd d", Ar) + " a las " + proximo.Hora + ".");
            }
            else if (deHoy.Count == 1)
                texto = "Hoy hay un turno, a las " + deHoy[0].Hora + ".";
            else
                texto = "Hoy hay " + deHoy.Count + " turnos, de " + deHoy.First().Hora + " a " + deHoy.Last().Hora + ".";

            if (pendientes == 1) texto += " Hay un pedido esperando confirmación.";
            else if (pendientes > 1) texto += " Hay " + pendientes + " pedidos esperando confirmación.";
            return texto;
        }

        private Fila Armar(Turno t)
        {
            string paciente;
            if (!nombres.TryGetValue(t.IdUsuarioPaciente, out paciente)) paciente = t.NombrePaciente;
            return new Fila
            {
                Fecha = Mayuscula(t.Fecha.ToString("ddd d/M", Ar).Replace(".", "")),
                Hora = t.Hora,
                Paciente = paciente,
                Especialidad = t.Especialidad,
                Medico = MedicoLegible(t.NombreMedico),
                Estado = t.Estado
            };
        }

        // "Rios, Julian" -> "Dr/a. Rios". Si se pidio "sin preferencia" todavia no tiene medico.
        private static string MedicoLegible(string nombre)
        {
            if (string.IsNullOrEmpty(nombre) || nombre == "-") return "médico a asignar";
            return "Dr/a. " + nombre.Split(',')[0];
        }

        private static string Rol(string rol, int cantidad)
        {
            switch (rol)
            {
                case "Paciente": return cantidad == 1 ? "paciente" : "pacientes";
                case "Medico": return cantidad == 1 ? "médico" : "médicos";
                default: return "de " + rol.ToLower();
            }
        }

        private static string Saludo()
        {
            int hora = DateTime.Now.Hour;
            if (hora < 6 || hora >= 20) return "Buenas noches";
            if (hora < 13) return "Buen día";
            return "Buenas tardes";
        }

        private static string Mayuscula(string s)
        {
            return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
