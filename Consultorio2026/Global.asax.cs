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
    }
}