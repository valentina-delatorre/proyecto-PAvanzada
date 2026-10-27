using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Especialistas
{
    public partial class ListadoEspecialistas : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarMedicos();
                CargarUsuariosSinFicha();
            }
        }

        private void CargarMedicos()
        {
            List<Medico> medicos = MedicoDatos.ListarTodos();
            GvMedicos.DataSource = medicos;
            GvMedicos.DataBind();

            // Los numeros de la cabecera
            List<Medico> activos = medicos.Where(m => m.Activo).ToList();
            LtActivos.Text = activos.Count.ToString();
            LtEspecialidades.Text = activos.Select(m => m.Especialidad).Distinct().Count().ToString();
            LtSemana.Text = TurnoDatos.ListarTodos().Count(t => t.IdMedico.HasValue && t.Estado != "Rechazado"
                && t.Fecha.Date >= DateTime.Today && t.Fecha.Date < DateTime.Today.AddDays(7)).ToString();
        }

        private void CargarUsuariosSinFicha()
        {
            DdlUsuarioVincular.Items.Clear();
            DdlUsuarioVincular.Items.Add(new System.Web.UI.WebControls.ListItem("Sin usuario (solo cartilla)", "0"));
            foreach (Usuario u in UsuarioDatos.ListarMedicosSinFicha())
                DdlUsuarioVincular.Items.Add(new System.Web.UI.WebControls.ListItem(u.NombreUsuario, u.IdUsuario.ToString()));
        }

        private Medico MedicoSeleccionado()
        {
            int id = (int)GvMedicos.SelectedDataKey.Value;
            return MedicoDatos.ListarTodos().Find(m => m.IdMedico == id);
        }

        protected void GvMedicos_SelectedIndexChanged(object sender, EventArgs e)
        {
            MostrarAgenda();
        }

        private void MostrarAgenda()
        {
            Medico m = MedicoSeleccionado();
            if (m == null) { PnAgenda.Visible = false; return; }

            PnAgenda.Visible = true;
            List<Turno> agenda = TurnoDatos.ListarTodosPorMedico(m.IdMedico);
            int proximos = agenda.Count(t => t.Fecha.Date >= DateTime.Today && t.Estado != "Rechazado");

            LtNombreMedico.Text = "Dr/a. " + HttpUtility.HtmlEncode(m.Nombre + " " + m.Apellido);
            LtFicha.Text = "<dl class=\"ficha\">"
                + "<div><dt>Especialidad</dt><dd>" + HttpUtility.HtmlEncode(m.Especialidad) + "</dd></div>"
                + "<div><dt>Usuario</dt><dd>" + HttpUtility.HtmlEncode(m.UsuarioVinculado) + "</dd></div>"
                + "<div><dt>Estado</dt><dd>" + (m.Activo ? "Atiende" : "Dado de baja") + "</dd></div>"
                + "<div><dt>Turnos próximos</dt><dd>" + proximos + "</dd></div>"
                + "</dl>";
            BtCambiarActivoMedico.Text = m.Activo ? "Dar de baja" : "Reactivar";
            BtCambiarActivoMedico.CssClass = m.Activo ? "btn btn-mini btn-peligro" : "btn btn-mini";

            GvAgenda.DataSource = agenda;
            GvAgenda.DataBind();
        }

        protected void BtCambiarActivoMedico_Click(object sender, EventArgs e)
        {
            Medico m = MedicoSeleccionado();
            if (m == null) return;

            MedicoDatos.CambiarActivo(m.IdMedico, !m.Activo);
            CargarMedicos();
            MostrarAgenda();
        }

        protected void BtAgregarMedico_Click(object sender, EventArgs e)
        {
            LbMensaje.Text = "";
            LbError.Text = "";

            if (TxtNombre.Text.Trim() == "" || TxtApellido.Text.Trim() == "" || TxtEspecialidad.Text.Trim() == "")
            {
                LbError.Text = "Completá nombre, apellido y especialidad.";
                return;
            }

            Medico nuevo = new Medico();
            nuevo.Nombre = TxtNombre.Text.Trim();
            nuevo.Apellido = TxtApellido.Text.Trim();
            nuevo.Especialidad = TxtEspecialidad.Text.Trim();
            nuevo.IdUsuario = int.Parse(DdlUsuarioVincular.SelectedValue);

            MedicoDatos.Insertar(nuevo);

            LbMensaje.Text = "✓ Especialista agregado.";
            TxtNombre.Text = ""; TxtApellido.Text = ""; TxtEspecialidad.Text = "";
            CargarMedicos();
            CargarUsuariosSinFicha();
        }
    }
}