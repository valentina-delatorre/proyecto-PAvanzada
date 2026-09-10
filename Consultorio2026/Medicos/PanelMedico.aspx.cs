using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Medicos
{
    public partial class PanelMedico : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarPanel();
        }

        private Medico MedicoActual()
        {
            return MedicoDatos.ObtenerPorUsuario((int)Session["IdUsuario"]);
        }

        private void CargarPanel()
        {
            Medico medico = MedicoActual();

            if (medico == null)
            {
                LbBienvenida.Text = "Tu usuario no está vinculado a ninguna ficha de médico. Avisale al administrador.";
                PnPanel.Visible = false;
                return;
            }

            LbBienvenida.Text = "Dr/a. <strong>" + medico.NombreCompleto + "</strong> — " + medico.Especialidad;

            List<Turno> pendientes = TurnoDatos.ListarPendientesPorMedico(medico.IdMedico);
            List<Turno> agenda = TurnoDatos.ListarPorMedico(medico.IdMedico);

            LtPendientes.Text = pendientes.Count.ToString();
            LtHoy.Text = agenda.Count(t => t.Fecha.Date == DateTime.Today).ToString();
            LtProximos.Text = agenda.Count(t => t.Fecha.Date > DateTime.Today).ToString();

            GvPendientes.DataSource = pendientes;
            GvPendientes.DataBind();

            GvAgenda.DataSource = agenda;
            GvAgenda.DataBind();
        }

        protected void GvPendientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idTurno = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "Aceptar")
                TurnoDatos.Resolver(idTurno, null, "Confirmado");
            else if (e.CommandName == "Rechazar")
                TurnoDatos.Resolver(idTurno, null, "Rechazado");

            CargarPanel();
        }

        // Convierte "Próximo"/"Atendido"/etc. en una clase CSS para el badge de color
        protected string ClaseEstado(string estadoVisual)
        {
            switch (estadoVisual)
            {
                case "Próximo": return "badge badge-proximo";
                case "Atendido": return "badge badge-atendido";
                case "Rechazado": return "badge badge-rechazado";
                default: return "badge badge-solicitado";
            }
        }
    }
}