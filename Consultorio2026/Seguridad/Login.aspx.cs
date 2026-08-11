using System;
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
                Session["IdUsuario"] = usuario.IdUsuario;
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