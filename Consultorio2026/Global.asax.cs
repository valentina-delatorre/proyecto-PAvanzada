using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Consultorio2026
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                if (ticket != null)
                {
                    string[] roles = new string[] { ticket.UserData };
                    var identidad = new FormsIdentity(ticket);
                    var principal = new System.Security.Principal.GenericPrincipal(identidad, roles);
                    Context.User = principal;
                }
            }
        }

        // Portero de los modulos. Corre en cada pedido, cuando ya hay sesion.
        // Antes cada carpeta decia en su Web.config que ROL podia entrar; ahora lo decide
        // la base (tabla URL + vista VW_ACCESOS), asi Seguridad puede habilitar o
        // deshabilitar un modulo a una persona puntual desde Seguridad/Accesos.aspx.
        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated || Context.Session == null || Session["IdUsuario"] == null)
                return;

            string ruta = Request.AppRelativeCurrentExecutionFilePath;
            if (BIZ.Datos.AccesoDatos.PuedeEntrar((int)Session["IdUsuario"], ruta))
                return;

            // El asistente se llama desde JavaScript: le contestamos un error, no una pagina
            if (ruta.EndsWith(".ashx", StringComparison.OrdinalIgnoreCase))
            {
                Response.StatusCode = 403;
                Response.End();
                return;
            }

            Response.Redirect("~/Inicio.aspx?sinAcceso=1");
        }
    }
}