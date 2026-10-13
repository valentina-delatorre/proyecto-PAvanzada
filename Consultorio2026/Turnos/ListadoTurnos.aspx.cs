using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Turnos
{
    public partial class ListadoTurnos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFiltros();
                CargarSolicitudes();
            }
        }

        private void CargarSolicitudes()
        {
            GvSolicitudes.DataSource = TurnoDatos.ListarSolicitados();
            GvSolicitudes.DataBind();
            CargarHistorial();
        }

        /* ----- Gestionar una solicitud ------------------------------------- */

        protected void GvSolicitudes_SelectedIndexChanged(object sender, EventArgs e)
        {
            PnGestion.Visible = true;

            List<Turno> solicitados = TurnoDatos.ListarSolicitados();
            int idTurno = (int)GvSolicitudes.SelectedDataKey.Value;
            Turno seleccionado = solicitados.Find(t => t.IdTurno == idTurno);

            if (seleccionado == null) { PnGestion.Visible = false; return; }

            LbDetalle.Text = seleccionado.NombrePaciente + " — " + seleccionado.Especialidad +
                             " — " + seleccionado.Fecha.ToString("dd/MM/yyyy") + " " + seleccionado.Hora;

            // El almanaque se dibuja sobre el mes del turno pedido
            ViewState["FechaPedida"] = seleccionado.Fecha;

            DdlMedicoAsignar.Items.Clear();
            foreach (Medico m in MedicoDatos.ListarPorEspecialidad(seleccionado.Especialidad))
                DdlMedicoAsignar.Items.Add(new ListItem(m.NombreCompleto, m.IdMedico.ToString()));

            // Si el paciente pidió un médico en particular, lo dejamos elegido
            if (seleccionado.IdMedico.HasValue && DdlMedicoAsignar.Items.FindByValue(seleccionado.IdMedico.ToString()) != null)
                DdlMedicoAsignar.SelectedValue = seleccionado.IdMedico.ToString();

            CargarCalendario();
        }

        protected void DdlMedicoAsignar_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarCalendario();
        }

        // Cada celda del almanaque. Numero vacío = relleno antes del día 1.
        public class Celda
        {
            public string Numero { get; set; }
            public string Horarios { get; set; }
            public string Clase { get; set; }
        }

        private void CargarCalendario()
        {
            if (DdlMedicoAsignar.Items.Count == 0 || ViewState["FechaPedida"] == null)
            {
                PnCalendario.Visible = false;
                return;
            }

            int idMedico = int.Parse(DdlMedicoAsignar.SelectedValue);
            DateTime pedida = (DateTime)ViewState["FechaPedida"];

            // El procedimiento almacenado arma el mes completo del médico
            List<DiaAgenda> dias = TurnoDatos.AgendaMes(idMedico, pedida.Year, pedida.Month);
            List<Celda> celdas = new List<Celda>();

            // Relleno para que el día 1 caiga en su columna (la semana arranca el lunes)
            DateTime primero = new DateTime(pedida.Year, pedida.Month, 1);
            int corrimiento = ((int)primero.DayOfWeek + 6) % 7;
            for (int i = 0; i < corrimiento; i++)
                celdas.Add(new Celda { Numero = "", Horarios = "", Clase = "cal-dia cal-vacio" });

            foreach (DiaAgenda d in dias)
            {
                string clase = "cal-dia";
                if (d.Turnos > 0) clase += " cal-ocupado";
                if (d.Fecha.Date == pedida.Date) clase += " cal-pedido";
                celdas.Add(new Celda { Numero = d.Dia.ToString(), Horarios = d.Horarios, Clase = clase });
            }

            LtMedicoAgenda.Text = DdlMedicoAsignar.SelectedItem.Text;
            LtMesAgenda.Text = pedida.ToString("MMMM yyyy", new CultureInfo("es-AR"));
            RpCalendario.DataSource = celdas;
            RpCalendario.DataBind();
            PnCalendario.Visible = true;
        }

        protected void BtConfirmar_Click(object sender, EventArgs e)
        {
            if (DdlMedicoAsignar.Items.Count == 0)
            {
                LbError.Text = "No hay médicos de esa especialidad para asignar.";
                return;
            }

            int idTurno = (int)GvSolicitudes.SelectedDataKey.Value;
            int idMedico = int.Parse(DdlMedicoAsignar.SelectedValue);

            TurnoDatos.Resolver(idTurno, idMedico, "Confirmado");

            PnGestion.Visible = false;
            CargarSolicitudes();
        }

        protected void BtRechazar_Click(object sender, EventArgs e)
        {
            int idTurno = (int)GvSolicitudes.SelectedDataKey.Value;
            TurnoDatos.Resolver(idTurno, null, "Rechazado");
            PnGestion.Visible = false;
            CargarSolicitudes();
        }

        /* ----- Historial con filtros --------------------------------------- */

        private void CargarFiltros()
        {
            DdlFiltroEstado.Items.Clear();
            DdlFiltroEstado.Items.Add(new ListItem("Todos", ""));
            foreach (string estado in new[] { "Solicitado", "Confirmado", "Rechazado", "Cancelado" })
                DdlFiltroEstado.Items.Add(estado);

            DdlFiltroEspecialidad.Items.Clear();
            DdlFiltroEspecialidad.Items.Add(new ListItem("Todas", ""));
            foreach (string esp in MedicoDatos.ListarEspecialidades())
                DdlFiltroEspecialidad.Items.Add(esp);
        }

        private void CargarHistorial()
        {
            List<Turno> todos = TurnoDatos.ListarTodos();

            string estado = DdlFiltroEstado.SelectedValue;
            string especialidad = DdlFiltroEspecialidad.SelectedValue;
            string paciente = TxtFiltroPaciente.Text.Trim();
            DateTime desde, hasta;
            bool hayDesde = DateTime.TryParse(TxtFiltroDesde.Text, out desde);
            bool hayHasta = DateTime.TryParse(TxtFiltroHasta.Text, out hasta);

            List<Turno> filtrados = todos.FindAll(t =>
                (estado == "" || t.Estado == estado) &&
                (especialidad == "" || t.Especialidad == especialidad) &&
                (paciente == "" || t.NombrePaciente.IndexOf(paciente, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (!hayDesde || t.Fecha.Date >= desde.Date) &&
                (!hayHasta || t.Fecha.Date <= hasta.Date));

            LbResultado.Text = filtrados.Count + " de " + todos.Count + " turnos.";
            GvHistorial.DataSource = filtrados;
            GvHistorial.DataBind();
        }

        protected void BtFiltrar_Click(object sender, EventArgs e)
        {
            CargarHistorial();
        }

        protected void BtLimpiar_Click(object sender, EventArgs e)
        {
            DdlFiltroEstado.SelectedIndex = 0;
            DdlFiltroEspecialidad.SelectedIndex = 0;
            TxtFiltroPaciente.Text = "";
            TxtFiltroDesde.Text = "";
            TxtFiltroHasta.Text = "";
            CargarHistorial();
        }
    }
}
