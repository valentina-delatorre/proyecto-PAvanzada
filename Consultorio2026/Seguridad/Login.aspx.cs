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

                Session["IdUsuario"] = usuario.IdUsuario;
                Session["NombreUsuario"] = usuario.NombreUsuario;
                Session["Rol"] = usuario.Rol;

                // Si venía de "Agendar turno", lo devolvemos exactamente a donde iba
                string returnUrl = Request.QueryString["ReturnUrl"];
                if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/")
                    && usuario.Rol == BIZ.Modelo.Roles.Paciente)
                {
                    Response.Redirect(returnUrl);
                    return;
                }

                if (usuario.Rol == BIZ.Modelo.Roles.Usuarios)
                    Response.Redirect("~/Usuarios/ListadoUsuarios.aspx");
                else if (usuario.Rol == BIZ.Modelo.Roles.Especialistas)
                    Response.Redirect("~/Especialistas/ListadoEspecialistas.aspx");
                else if (usuario.Rol == BIZ.Modelo.Roles.Turnos)
                    Response.Redirect("~/Turnos/ListadoTurnos.aspx");
                else if (usuario.Rol == BIZ.Modelo.Roles.Reportes)
                    Response.Redirect("~/Reportes/ReporteDiario.aspx");
                else if (usuario.Rol == BIZ.Modelo.Roles.Medico)
                    Response.Redirect("~/Medicos/PanelMedico.aspx");
                else if (usuario.Rol == BIZ.Modelo.Roles.Paciente)
                    Response.Redirect("~/Portal/PortalPaciente.aspx");
                else
                    Response.Redirect("~/Inicio.aspx");
            }
            else
            {
                LbError.Text = "Usuario o contraseña incorrectos.";
            }
        }
    }
}