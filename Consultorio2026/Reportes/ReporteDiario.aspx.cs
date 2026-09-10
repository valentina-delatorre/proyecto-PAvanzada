using System;
using BIZ.Datos;

namespace Consultorio2026.Reportes
{
    public partial class ReporteDiario : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LtFechaHoy.Text = DateTime.Today.ToString("dd/MM/yyyy");

                GvHoy.DataSource = TurnoDatos.ListarDelDia();
                GvHoy.DataBind();

                GvEstados.DataSource = ReporteDatos.TotalesPorEstado();
                GvEstados.DataBind();

                GvEspecialidades.DataSource = ReporteDatos.TurnosPorEspecialidad();
                GvEspecialidades.DataBind();

                GvProximos.DataSource = ReporteDatos.ProximosPorMedico();
                GvProximos.DataBind();
            }
        }
    }
}