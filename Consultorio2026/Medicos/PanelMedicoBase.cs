using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Medicos
{
    // Lo que comparten todos los paneles de medico: identificar al medico logueado,
    // mostrar sus solicitudes y su agenda, y aceptar o rechazar turnos.
    // Cada especialidad tiene su propia pagina que hereda de aca.
    public abstract class PanelMedicoBase : System.Web.UI.Page
    {
        protected Label LbBienvenida;
        protected Panel PnPanel;
        protected Literal LtHoy;
        protected Literal LtProximos;
        protected Literal LtPendientes;
        protected GridView GvPendientes;
        protected GridView GvAgenda;

        // Especialidad que atiende este panel. Null = panel generico.
        protected abstract string EspecialidadDelPanel { get; }

        // Que panel le toca a cada especialidad. Las que no figuran usan el generico.
        private static readonly Dictionary<string, string> Paneles = new Dictionary<string, string>
        {
            { "Clinica Medica", "~/Medicos/PanelClinica.aspx" },
            { "Cardiologia",    "~/Medicos/PanelCardiologia.aspx" },
            { "Pediatria",      "~/Medicos/PanelPediatria.aspx" },
            { "Traumatologia",  "~/Medicos/PanelTraumatologia.aspx" }
        };

        public static string RutaPanel(string especialidad)
        {
            string ruta;
            if (especialidad != null && Paneles.TryGetValue(especialidad, out ruta))
                return ruta;
            return "~/Medicos/PanelMedico.aspx";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            Medico medico = MedicoActual();

            // Si el medico entro al panel de otra especialidad, lo mandamos al suyo
            if (medico != null && RutaPanel(medico.Especialidad) != RutaPanel(EspecialidadDelPanel))
            {
                Response.Redirect(RutaPanel(medico.Especialidad));
                return;
            }

            CargarPanel(medico);
        }

        private Medico MedicoActual()
        {
            return MedicoDatos.ObtenerPorUsuario((int)Session["IdUsuario"]);
        }

        private void CargarPanel(Medico medico)
        {
            if (medico == null)
            {
                LbBienvenida.Text = "Tu usuario no tiene ficha de médico. Avisale al administrador.";
                PnPanel.Visible = false;
                return;
            }

            LbBienvenida.Text = "Dr/a. " + System.Web.HttpUtility.HtmlEncode(medico.Nombre + " " + medico.Apellido);

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

            CargarPanel(MedicoActual());
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
