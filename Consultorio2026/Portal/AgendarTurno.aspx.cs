using System;
using System.Collections.Generic;
using System.Globalization;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Portal
{
    public partial class AgendarTurno : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEspecialidades();
                PreseleccionarServicio();
                CargarMedicos();
                CargarDatosGuardados();
            }
        }

        private void CargarEspecialidades()
        {
            DdlEspecialidad.DataSource = MedicoDatos.ListarEspecialidades();
            DdlEspecialidad.DataBind();
        }

        private void PreseleccionarServicio()
        {
            // Si vino desde el detalle de un servicio (?servicio=N), preseleccionamos su especialidad
            int idServicio;
            if (int.TryParse(Request.QueryString["servicio"], out idServicio))
            {
                ServicioInfo servicio = ServiciosCatalogo.Obtener(idServicio);
                if (servicio != null && DdlEspecialidad.Items.FindByValue(servicio.Especialidad) != null)
                    DdlEspecialidad.SelectedValue = servicio.Especialidad;
            }
        }

        private void CargarMedicos()
        {
            List<Medico> medicos = MedicoDatos.ListarPorEspecialidad(DdlEspecialidad.SelectedValue);
            DdlMedico.Items.Clear();
            DdlMedico.Items.Add(new System.Web.UI.WebControls.ListItem("Cualquier especialista", "0"));
            foreach (Medico m in medicos)
                DdlMedico.Items.Add(new System.Web.UI.WebControls.ListItem(m.NombreCompleto, m.IdMedico.ToString()));
        }

        private void CargarDatosGuardados()
        {
            // Si ya agendó antes, sus datos personales vienen precargados
            DatoPaciente datos = DatoPacienteDatos.Obtener((int)Session["IdUsuario"]);
            if (datos != null)
            {
                TxtNombre.Text = datos.Nombre;
                TxtApellido.Text = datos.Apellido;
                TxtDni.Text = datos.Dni;
                TxtTelefono.Text = datos.Telefono;
                TxtEmail.Text = datos.Email;
            }
        }

        protected void DdlEspecialidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarMedicos();
            CargarCalendario();
        }

        protected void DdlMedico_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarCalendario();
        }

        protected void TxtFecha_TextChanged(object sender, EventArgs e)
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
            // El procedimiento es por profesional: con "Cualquier especialista" no hay agenda que mostrar
            if (DdlMedico.SelectedValue == "0")
            {
                PnCalendario.Visible = false;
                LbSinAgenda.Visible = true;
                return;
            }

            // Se muestra el mes de la fecha elegida, o el actual si todavía no eligió
            DateTime elegida;
            bool hayFecha = DateTime.TryParse(TxtFecha.Text, out elegida);
            DateTime mes = hayFecha ? elegida : DateTime.Today;

            List<DiaAgenda> dias = TurnoDatos.AgendaMes(int.Parse(DdlMedico.SelectedValue), mes.Year, mes.Month);
            List<Celda> celdas = new List<Celda>();

            // Relleno para que el día 1 caiga en su columna (la semana arranca el lunes)
            DateTime primero = new DateTime(mes.Year, mes.Month, 1);
            int corrimiento = ((int)primero.DayOfWeek + 6) % 7;
            for (int i = 0; i < corrimiento; i++)
                celdas.Add(new Celda { Numero = "", Horarios = "", Clase = "cal-dia cal-vacio" });

            // Un día está completo cuando tiene tomados todos los horarios que se ofrecen
            int horariosPorDia = DdlHora.Items.Count;

            foreach (DiaAgenda d in dias)
            {
                string clase = "cal-dia";
                if (d.Fecha.Date < DateTime.Today) clase += " cal-pasado";
                else if (d.Turnos >= horariosPorDia) clase += " cal-completo";
                else if (d.Turnos > 0) clase += " cal-parcial";
                else clase += " cal-libre";
                if (hayFecha && d.Fecha.Date == elegida.Date) clase += " cal-pedido";

                string horarios = d.Turnos > 0 && d.Fecha.Date >= DateTime.Today ? "Ocupado: " + d.Horarios : "";
                celdas.Add(new Celda { Numero = d.Dia.ToString(), Horarios = horarios, Clase = clase });
            }

            LtMedicoAgenda.Text = DdlMedico.SelectedItem.Text;
            LtMesAgenda.Text = mes.ToString("MMMM yyyy", new CultureInfo("es-AR"));
            RpCalendario.DataSource = celdas;
            RpCalendario.DataBind();
            PnCalendario.Visible = true;
            LbSinAgenda.Visible = false;
        }

        protected void BtConfirmar_Click(object sender, EventArgs e)
        {
            LbMensaje.Text = "";
            LbError.Text = "";

            if (TxtNombre.Text.Trim() == "" || TxtApellido.Text.Trim() == "" || TxtDni.Text.Trim() == "")
            {
                LbError.Text = "Completá nombre, apellido y DNI.";
                return;
            }

            DateTime fecha;
            if (!DateTime.TryParse(TxtFecha.Text, out fecha) || fecha.Date < DateTime.Today)
            {
                LbError.Text = "Elegí una fecha válida (de hoy en adelante).";
                return;
            }

            int idUsuario = (int)Session["IdUsuario"];

            // 1. Guardamos (o actualizamos) los datos personales
            DatoPaciente datos = new DatoPaciente();
            datos.IdUsuario = idUsuario;
            datos.Nombre = TxtNombre.Text.Trim();
            datos.Apellido = TxtApellido.Text.Trim();
            datos.Dni = TxtDni.Text.Trim();
            datos.Telefono = TxtTelefono.Text.Trim();
            datos.Email = TxtEmail.Text.Trim();
            DatoPacienteDatos.Guardar(datos);

            // 2. Creamos la solicitud de turno
            Turno t = new Turno();
            t.IdUsuarioPaciente = idUsuario;
            t.IdMedico = DdlMedico.SelectedValue == "0" ? (int?)null : int.Parse(DdlMedico.SelectedValue);
            t.Especialidad = DdlEspecialidad.SelectedValue;
            t.TipoConsulta = DdlTipoConsulta.SelectedValue;
            t.Fecha = fecha;
            t.Hora = DdlHora.SelectedValue;
            t.Observaciones = TxtObservaciones.Text.Trim();
            TurnoDatos.Solicitar(t);

            LbMensaje.Text = "✓ Turno solicitado correctamente. Te va a figurar como Confirmado cuando el consultorio lo apruebe.";
            LkMisTurnos.Visible = true;
        }
    }
}