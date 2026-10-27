using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using BIZ.Datos;
using BIZ.Modelo;

/*
   PANTALLA "MIS TURNOS" DEL PACIENTE

   Es lo primero que ve el paciente cuando entra. Tiene dos partes:
     1. Solicitar un turno: elige especialidad, médico y motivo, y en un almanaque
        ve qué días tienen lugar. Toca un día, le aparecen las horas libres, elige una
        y aprieta "Solicitar turno".
     2. Mis turnos: la lista de los turnos que ya pidió, con su estado.

   Cómo funciona por dentro, en corto:
     - Cada vez que el paciente toca algo (cambia la especialidad, toca un día, cambia
       de mes...) la página vuelve al servidor (eso es un "postback"), se ejecuta el
       método que corresponde y la página se vuelve a dibujar.
     - Casi todo termina llamando a CargarCalendario(), que es el corazón de la pantalla:
       le pregunta a la base cómo está la agenda del mes y pinta cada día.
     - La agenda la arma el procedimiento almacenado SP_AGENDA_MES (en la base),
       que se llama desde TurnoDatos.AgendaMes().
*/

namespace Consultorio2026.Portal
{
    public partial class PortalPaciente : System.Web.UI.Page
    {
        // Se ejecuta cada vez que se carga la página.
        // IsPostBack es false solo la PRIMERA vez que el paciente entra: ahí llenamos todo.
        // En los postbacks (cuando toca algo) no hace falta, porque ASP.NET se acuerda
        // de lo que tenían los controles gracias al ViewState.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEspecialidades();
                CargarMedicos();
                CargarCalendario();
                CargarMisTurnos();
            }
        }

        // Llena el desplegable de especialidades (Cardiologia, Pediatria...) con lo que hay en la base.
        private void CargarEspecialidades()
        {
            DdlEspecialidad.DataSource = MedicoDatos.ListarEspecialidades();
            DdlEspecialidad.DataBind();
        }

        // Llena el desplegable de médicos con los de la especialidad elegida.
        // La primera opción es "Sin preferencia" con valor "0": significa "cualquiera me sirve".
        private void CargarMedicos()
        {
            List<Medico> medicos = MedicoDatos.ListarPorEspecialidad(DdlEspecialidad.SelectedValue);
            DdlMedico.Items.Clear();
            DdlMedico.Items.Add(new ListItem("Sin preferencia", "0"));
            foreach (Medico m in medicos)
                DdlMedico.Items.Add(new ListItem(m.NombreCompleto, m.IdMedico.ToString()));   // se ve el nombre, se guarda el id
        }

        // Trae de la base los turnos de ESTE paciente y los muestra en la lista de "Mis turnos".
        // El id del paciente sale de la sesión: se guardó ahí cuando hizo login.
        private void CargarMisTurnos()
        {
            int idUsuario = (int)Session["IdUsuario"];
            List<Turno> turnos = TurnoDatos.ListarPorPaciente(idUsuario);
            RpMisTurnos.DataSource = turnos;
            RpMisTurnos.DataBind();                    // el Repeater repite el HTML del .aspx una vez por turno
            LbSinTurnos.Visible = turnos.Count == 0;   // si no tiene ninguno, mostramos el cartelito

            // La cabecera: "Hola, Carla" (el nombre real sale de la tabla PERSONA) y dos números:
            // los turnos que tiene de hoy en adelante, y cuántos todavía no confirmó el consultorio.
            string nombre;
            UsuarioDatos.NombresReales().TryGetValue(idUsuario, out nombre);
            LtSaludo.Text = "Hola, " + System.Web.HttpUtility.HtmlEncode((nombre ?? Session["NombreUsuario"].ToString()).Split(' ')[0]);
            List<Turno> futuros = turnos.FindAll(t => t.Fecha.Date >= DateTime.Today && t.Estado != "Rechazado");
            LtProximos.Text = futuros.Count.ToString();
            LtEsperando.Text = futuros.FindAll(t => t.Estado == "Solicitado").Count.ToString();
        }

        // Estos dos métodos los usa el .aspx desde adentro del Repeater de "Mis turnos"
        // para mostrar los datos más lindos. Son "protected" para que el .aspx los pueda ver.

        // Convierte una fecha en el mes abreviado en mayúsculas: 15/09/2026 -> "SEP".
        protected string Mes(object fecha)
        {
            return ((DateTime)fecha).ToString("MMM", new CultureInfo("es-AR")).Substring(0, 3).ToUpper();
        }

        // Si el paciente pidió "Sin preferencia", el turno todavía no tiene médico
        // (lo asigna el consultorio cuando lo confirma). En vez de dejar el hueco vacío, lo decimos.
        protected string Medico(object nombre)
        {
            return nombre == null || nombre.ToString() == "" || nombre.ToString() == "-" ? "Médico a asignar" : nombre.ToString();
        }

        /* ----- Lo que pasa cuando el paciente toca algo ------------------------ */

        // Cambió la especialidad: cambian los médicos posibles, y por lo tanto la agenda.
        protected void DdlEspecialidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarMedicos();
            CargarCalendario();
        }

        // Cambió el médico: hay que mostrar la agenda de ese médico.
        protected void DdlMedico_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarCalendario();
        }

        // Tocó una de las flechas ‹ › del almanaque.
        // Las dos flechas usan este mismo método: en el .aspx cada una tiene un CommandArgument
        // distinto, "-1" la de atrás y "1" la de adelante. Así sabemos para dónde moverse.
        protected void BtMes_Command(object sender, CommandEventArgs e)
        {
            MesVisible = MesVisible.AddMonths(int.Parse(e.CommandArgument.ToString()));
            CargarCalendario();
        }

        // Tocó un día del almanaque.
        // Cada día es un botón y lleva su fecha en CommandArgument (ej: "2026-11-12").
        // La guardamos en un campo oculto (HfFecha) para acordarnos de qué día eligió,
        // y redibujamos el almanaque para que ese día quede marcado y aparezcan sus horas.
        protected void RpCalendario_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            HfFecha.Value = e.CommandArgument.ToString();
            CargarCalendario();
        }

        /* ----- Almanaque de turnos disponibles ----------------------------- */

        // El mes que el paciente está mirando en el almanaque.
        // Problema: entre un click y otro la página "se olvida" de todo lo que no esté en un control.
        // Solución: lo guardamos en el ViewState, que es una memoria que viaja escondida dentro de la página.
        // Siempre se guarda como el día 1 de ese mes. Si todavía no hay nada guardado, arranca en el mes actual
        // (hoy menos los días que ya pasaron del mes = el día 1).
        private DateTime MesVisible
        {
            get { return ViewState["Mes"] == null ? DateTime.Today.AddDays(1 - DateTime.Today.Day) : (DateTime)ViewState["Mes"]; }
            set { ViewState["Mes"] = new DateTime(value.Year, value.Month, 1); }
        }

        // Los horarios que atiende el consultorio. Cada médico tiene estos 7 turnos por día.
        private static readonly string[] Horarios = { "09:00", "10:00", "11:00", "12:00", "15:00", "16:00", "17:00" };

        // Un casillero del almanaque. Es lo que le pasamos al Repeater para que dibuje cada día.
        //   Numero        -> el número que se ve (12)
        //   Texto         -> lo que va abajo ("5 turnos disponibles", "Completo"...)
        //   Clase         -> las clases CSS que le dan el color (cal-libre, cal-completo...)
        //   Fecha         -> la fecha completa, viaja como CommandArgument cuando lo tocan
        //   Seleccionable -> si se puede tocar (los días pasados y los completos no)
        // Los casilleros vacíos del principio (antes del día 1) no tienen número ni fecha.
        public class Celda
        {
            public string Numero { get; set; }
            public string Texto { get; set; }
            public string Clase { get; set; }
            public string Fecha { get; set; }
            public bool Seleccionable { get; set; }
        }

        // EL MÉTODO MÁS IMPORTANTE DE LA PANTALLA.
        // Arma el almanaque del mes: para cada día calcula cuántos turnos quedan libres y le pone un color.
        // Si el paciente ya tocó un día, además prepara las horas libres de ese día.
        private void CargarCalendario()
        {
            // PASO 1: ¿de qué médicos mostramos la agenda?
            // Si eligió un médico, solo ese. Si dejó "Sin preferencia", todos los de la especialidad
            // (y entonces los turnos libres se suman entre todos).
            // El "0" es "Sin preferencia", por eso lo salteamos: no es un médico.
            List<int> medicos = new List<int>();
            foreach (ListItem m in DdlMedico.Items)
                if (m.Value != "0" && (DdlMedico.SelectedValue == "0" || m.Selected))
                    medicos.Add(int.Parse(m.Value));

            // Si la especialidad no tiene ningún médico, no hay agenda para mostrar.
            if (medicos.Count == 0)
            {
                PnCalendario.Visible = false;
                return;
            }

            DateTime mes = MesVisible;

            // ¿Ya tocó algún día? Si en el campo oculto hay una fecha válida, hayFecha queda en true
            // y la fecha queda en "elegida". Si está vacío, hayFecha queda en false.
            DateTime elegida;
            bool hayFecha = DateTime.TryParse(HfFecha.Value, out elegida);

            // PASO 2: contar los turnos libres de cada día del mes.
            // Para cada médico llamamos al procedimiento almacenado SP_AGENDA_MES (a través de AgendaMes).
            // Nos devuelve una fila por cada día del mes con los horarios que ya están ocupados,
            // por ejemplo el día 1 con Horarios = "17:00".
            // Entonces, para cada día y cada horario nuestro: si NO aparece en los ocupados, está libre.
            //
            // libres[] es un contador por día: libres[0] es el día 1, libres[1] el día 2, etc.
            // (por eso se usa d.Dia - 1: los arrays empiezan en 0 y los días en 1).
            int cantidadDias = DateTime.DaysInMonth(mes.Year, mes.Month);   // 28, 29, 30 o 31
            int[] libres = new int[cantidadDias];
            List<string> horasDelDia = new List<string>();   // de paso, juntamos las horas libres del día que tocó
            foreach (int idMedico in medicos)
            {
                List<DiaAgenda> dias = TurnoDatos.AgendaMes(idMedico, mes.Year, mes.Month);
                foreach (DiaAgenda d in dias)
                    foreach (string h in Horarios)
                        if (!d.Horarios.Contains(h))   // este horario no está ocupado ese día
                        {
                            libres[d.Dia - 1]++;
                            // Si es el día que eligió el paciente, anotamos la hora (una sola vez,
                            // aunque la tengan libre varios médicos).
                            if (hayFecha && d.Fecha.Date == elegida.Date && !horasDelDia.Contains(h))
                                horasDelDia.Add(h);
                        }
            }

            // El máximo posible por día: 7 horarios por cada médico. Si un día tiene exactamente esto
            // libre, es porque nadie sacó turno todavía (se pinta verde).
            int totalPorDia = Horarios.Length * medicos.Count;

            List<Celda> celdas = new List<Celda>();

            // PASO 3: casilleros vacíos al principio.
            // El almanaque tiene 7 columnas que arrancan en lunes. Si el mes empieza un jueves,
            // hay que dejar 3 casilleros vacíos (lun, mar, mié) para que el 1 caiga debajo de "Jue".
            // DayOfWeek de C# cuenta domingo = 0, lunes = 1 ... sábado = 6. La cuenta (+6) % 7
            // lo corre para que lunes = 0 y domingo = 6, que es justo la cantidad de vacíos que hacen falta.
            int corrimiento = ((int)mes.DayOfWeek + 6) % 7;
            for (int i = 0; i < corrimiento; i++)
                celdas.Add(new Celda { Clase = "cal-dia cal-vacio" });

            // PASO 4: armar un casillero por cada día del mes.
            for (int dia = 1; dia <= cantidadDias; dia++)
            {
                DateTime fecha = new DateTime(mes.Year, mes.Month, dia);
                int disponibles = libres[dia - 1];

                // Elegimos el color (la clase CSS) y el texto de abajo:
                //   - ya pasó            -> gris, sin texto
                //   - no queda ninguno   -> rojo, "Completo"
                //   - está todo libre    -> verde, "7 turnos disponibles"
                //   - quedan algunos     -> amarillo, "3 turnos disponibles"
                string clase, texto = "";
                if (fecha < DateTime.Today) clase = "cal-pasado";
                else if (disponibles == 0) { clase = "cal-completo"; texto = "Completo"; }
                else
                {
                    clase = disponibles == totalPorDia ? "cal-libre" : "cal-parcial";
                    texto = disponibles + (disponibles == 1 ? " turno disponible" : " turnos disponibles");
                }

                // Si es el día que tocó el paciente, le sumamos la clase que lo pinta de verde oscuro.
                if (hayFecha && fecha == elegida.Date) clase += " cal-elegido";

                celdas.Add(new Celda
                {
                    Numero = dia.ToString(),
                    Texto = texto,
                    Clase = "cal-dia " + clase,
                    Fecha = fecha.ToString("yyyy-MM-dd"),
                    Seleccionable = fecha >= DateTime.Today && disponibles > 0   // solo se tocan días futuros con lugar
                });
            }

            // PASO 5: mostrar todo en pantalla.

            // La flecha de "mes anterior" se apaga si ya estamos en el mes actual (no tiene sentido ir al pasado).
            BtMesAnterior.Enabled = mes > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // El título: "Turnos de Rios, Julian" o "Turnos de Traumatologia (todos los médicos)".
            LtMedicoAgenda.Text = DdlMedico.SelectedValue == "0"
                ? DdlEspecialidad.SelectedValue + " (todos los médicos)"
                : DdlMedico.SelectedItem.Text;
            LtMesAgenda.Text = mes.ToString("MMMM yyyy", new CultureInfo("es-AR"));   // "noviembre 2026"

            // Las horas solo aparecen si el paciente tocó un día de ESTE mes que todavía tiene lugar.
            // (Si cambió de mes o de médico y ese día ya no tiene horas, el recuadro se esconde.)
            PnHoras.Visible = horasDelDia.Count > 0;
            if (PnHoras.Visible) CargarHoras(elegida, horasDelDia);

            // Le pasamos la lista de casilleros al Repeater y él dibuja un botón por cada uno.
            RpCalendario.DataSource = celdas;
            RpCalendario.DataBind();
            PnCalendario.Visible = true;
        }

        // Llena las "píldoras" de horario que aparecen debajo del almanaque cuando el paciente toca un día.
        // Solo pone las horas que están libres, y en el orden de la lista Horarios (09:00, 10:00...).
        private void CargarHoras(DateTime elegida, List<string> horasDelDia)
        {
            // Si ya había elegido una hora y sigue estando libre, se la dejamos marcada.
            string horaAnterior = RblHora.SelectedValue;
            RblHora.Items.Clear();
            foreach (string h in Horarios)
                if (horasDelDia.Contains(h)) RblHora.Items.Add(h);
            if (RblHora.Items.FindByValue(horaAnterior) != null)
                RblHora.SelectedValue = horaAnterior;

            // El título del recuadro: "Horarios libres del domingo 1 de noviembre".
            LbFechaElegida.Text = "Horarios libres del " + elegida.ToString("dddd d 'de' MMMM", new CultureInfo("es-AR"));
        }

        /* ----- Guardar el turno --------------------------------------------- */

        // Apretó "Solicitar turno".
        // Primero revisamos que haya elegido día y hora; si falta algo, mostramos el error y cortamos (return).
        // Si está todo, armamos el turno y lo guardamos en la base con estado "Solicitado".
        // Después el consultorio lo confirma o lo rechaza desde el módulo de Turnos.
        protected void BtSolicitar_Click(object sender, EventArgs e)
        {
            LbMensaje.Text = "";
            LbError.Text = "";

            // Si no está visible el recuadro de horas, es que no tocó ningún día (o el que tocó ya no tiene lugar).
            DateTime fecha;
            if (!PnHoras.Visible || !DateTime.TryParse(HfFecha.Value, out fecha) || fecha.Date < DateTime.Today)
            {
                LbError.Text = "Elegí un día en el almanaque.";
                return;
            }

            // SelectedIndex es -1 cuando no hay ninguna hora marcada.
            if (RblHora.SelectedIndex < 0)
            {
                LbError.Text = "Elegí un horario de ese día.";
                return;
            }

            Turno t = new Turno();
            t.IdUsuarioPaciente = (int)Session["IdUsuario"];
            // Con "Sin preferencia" el médico queda vacío (null): lo asigna el consultorio al confirmar.
            t.IdMedico = DdlMedico.SelectedValue == "0" ? (int?)null : int.Parse(DdlMedico.SelectedValue);
            t.Especialidad = DdlEspecialidad.SelectedValue;
            t.TipoConsulta = DdlTipoConsulta.SelectedValue;
            t.Fecha = fecha;
            t.Hora = RblHora.SelectedValue;
            t.Observaciones = "";

            TurnoDatos.Solicitar(t);   // el INSERT está en BIZ/Datos/TurnoDatos.cs

            LbMensaje.Text = "✓ Turno solicitado. Te va a aparecer como Confirmado cuando el consultorio lo apruebe.";
            CargarMisTurnos();   // recargamos la lista para que aparezca el turno nuevo
        }
    }
}
