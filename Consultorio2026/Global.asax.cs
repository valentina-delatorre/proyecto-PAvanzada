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
            if (Session != null && Session["Rol"] != null)
            {
                string rol = Session["Rol"].ToString();
                var identidad = new System.Security.Principal.GenericIdentity(Session["NombreUsuario"].ToString());
                var principal = new System.Security.Principal.GenericPrincipal(identidad, new string[] { rol });
                Context.User = principal;
            }
        }
    }
}