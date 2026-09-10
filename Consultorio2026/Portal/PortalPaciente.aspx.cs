using System;
using System.Collections.Generic;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Portal
{
    public partial class PortalPaciente : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEspecialidades();
                CargarMedicos();
                CargarMisTurnos();
            }
        }

        private void CargarEspecialidades()
        {
            DdlEspecialidad.DataSource = MedicoDatos.ListarEspecialidades();
            DdlEspecialidad.DataBind();
        }

        private void CargarMedicos()
        {
            List<Medico> medicos = MedicoDatos.ListarPorEspecialidad(DdlEspecialidad.SelectedValue);
            DdlMedico.Items.Clear();
            DdlMedico.Items.Add(new System.Web.UI.WebControls.ListItem("Sin preferencia", "0"));
            foreach (Medico m in medicos)
                DdlMedico.Items.Add(new System.Web.UI.WebControls.ListItem(m.NombreCompleto, m.IdMedico.ToString()));
        }

        private void CargarMisTurnos()
        {
            int idUsuario = (int)Session["IdUsuario"];
            GvMisTurnos.DataSource = TurnoDatos.ListarPorPaciente(idUsuario);
            GvMisTurnos.DataBind();
        }

        protected void DdlEspecialidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarMedicos();
        }

        protected void BtSolicitar_Click(object sender, EventArgs e)
        {
            LbMensaje.Text = "";
            LbError.Text = "";

            DateTime fecha;
            if (!DateTime.TryParse(TxtFecha.Text, out fecha) || fecha.Date < DateTime.Today)
            {
                LbError.Text = "Elegí una fecha válida (de hoy en adelante).";
                return;
            }

            Turno t = new Turno();
            t.IdUsuarioPaciente = (int)Session["IdUsuario"];
            t.IdMedico = DdlMedico.SelectedValue == "0" ? (int?)null : int.Parse(DdlMedico.SelectedValue);
            t.Especialidad = DdlEspecialidad.SelectedValue;
            t.TipoConsulta = DdlTipoConsulta.SelectedValue;
            t.Fecha = fecha;
            t.Hora = DdlHora.SelectedValue;
            t.Observaciones = TxtObservaciones.Text.Trim();

            TurnoDatos.Solicitar(t);

            LbMensaje.Text = "✓ Turno solicitado. Te va a aparecer como Confirmado cuando el consultorio lo apruebe.";
            CargarMisTurnos();
        }
    }
}