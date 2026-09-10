using System;
using System.Web;

namespace Consultorio2026
{
    public partial class DetalleServicio : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ServicioInfo servicio = ObtenerServicioActual();

                if (servicio == null)
                {
                    Response.Redirect("~/Servicios.aspx");
                    return;
                }

                LtNombre.Text = servicio.Nombre;
                LtDetalle.Text = servicio.Detalle;

                RpImagenes.DataSource = servicio.ImagenesDetalle;
                RpImagenes.DataBind();
            }
        }

        protected void BtAgendar_Click(object sender, EventArgs e)
        {
            ServicioInfo servicio = ObtenerServicioActual();
            if (servicio == null) { Response.Redirect("~/Servicios.aspx"); return; }

            string destino = "~/Portal/AgendarTurno.aspx?servicio=" + servicio.Id;

            if (Request.IsAuthenticated)
            {
                // Ya pasó por el login: va directo a agendar
                Response.Redirect(destino);
            }
            else
            {
                // No está logueado: primero el login, y ReturnUrl recuerda a dónde iba
                Response.Redirect("~/Seguridad/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(ResolveUrl(destino)));
            }
        }

        private ServicioInfo ObtenerServicioActual()
        {
            int id;
            if (!int.TryParse(Request.QueryString["id"], out id))
                return null;
            return ServiciosCatalogo.Obtener(id);
        }
    }
}