using System;
using System.Collections.Generic;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Turnos
{
    public partial class ListadoTurnos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarSolicitudes();
        }

        private void CargarSolicitudes()
        {
            GvSolicitudes.DataSource = TurnoDatos.ListarSolicitados();
            GvSolicitudes.DataBind();

            GvHistorial.DataSource = TurnoDatos.ListarTodos();
            GvHistorial.DataBind();
        }

        protected void GvSolicitudes_SelectedIndexChanged(object sender, EventArgs e)
        {
            PnGestion.Visible = true;

            List<Turno> solicitados = TurnoDatos.ListarSolicitados();
            int idTurno = (int)GvSolicitudes.SelectedDataKey.Value;
            Turno seleccionado = solicitados.Find(t => t.IdTurno == idTurno);

            if (seleccionado == null) { PnGestion.Visible = false; return; }

            LbDetalle.Text = seleccionado.NombrePaciente + " — " + seleccionado.Especialidad +
                             " — " + seleccionado.Fecha.ToString("dd/MM/yyyy") + " " + seleccionado.Hora;

            DdlMedicoAsignar.Items.Clear();
            foreach (Medico m in MedicoDatos.ListarPorEspecialidad(seleccionado.Especialidad))
                DdlMedicoAsignar.Items.Add(new System.Web.UI.WebControls.ListItem(m.NombreCompleto, m.IdMedico.ToString()));
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
    }
}