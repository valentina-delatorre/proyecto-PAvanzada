using System;
using System.Collections.Generic;
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
            GvMedicos.DataSource = MedicoDatos.ListarTodos();
            GvMedicos.DataBind();
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
            LbDetalleMedico.Text = "Dr/a. <strong>" + m.NombreCompleto + "</strong> — " + m.Especialidad +
                                   " — Usuario: " + m.UsuarioVinculado +
                                   " — Estado: " + (m.Activo ? "Activo" : "Dado de baja");
            BtCambiarActivoMedico.Text = m.Activo ? "Dar de baja" : "Reactivar";

            GvAgenda.DataSource = TurnoDatos.ListarTodosPorMedico(m.IdMedico);
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