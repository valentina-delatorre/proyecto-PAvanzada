using System;
using System.Collections.Generic;
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