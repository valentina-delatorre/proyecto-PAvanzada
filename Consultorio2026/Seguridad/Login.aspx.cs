using System;
using System.Web;
using System.Web.Security;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Seguridad
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void BtIngresar_Click(object sender, EventArgs e)
        {
            Usuario usuario = UsuarioDatos.ValidarLogin(TxtUsuario.Text, TxtContrasenia.Text);

            if (usuario != null)
            {
                // Creamos el "ticket" de autenticación, guardando el Rol adentro
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    usuario.NombreUsuario,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    usuario.Rol
                );

                string ticketCifrado = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, ticketCifrado);
                Response.Cookies.Add(cookie);

                // La Session la seguimos usando, pero solo para mostrar datos en pantalla
                Session["NombreUsuario"] = usuario.NombreUsuario;
                Session["Rol"] = usuario.Rol;

                Response.Redirect("~/Default.aspx");
            }
            else
            {
                LbError.Text = "Usuario o contraseña incorrectos.";
            }
        }
    }
}