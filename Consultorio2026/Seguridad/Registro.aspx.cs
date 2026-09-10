using System;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Seguridad
{
    public partial class Registro : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void BtRegistrarse_Click(object sender, EventArgs e)
        {
            string nombre = TxtUsuario.Text.Trim();

            if (nombre == "" || TxtContrasenia.Text == "")
            {
                LbError.Text = "Completá usuario y contraseña.";
                return;
            }

            if (TxtContrasenia.Text != TxtConfirmar.Text)
            {
                LbError.Text = "Las contraseñas no coinciden.";
                return;
            }

            if (UsuarioDatos.Existe(nombre))
            {
                LbError.Text = "Ese nombre de usuario ya está en uso.";
                return;
            }

            Usuario nuevo = new Usuario();
            nuevo.NombreUsuario = nombre;
            nuevo.Contrasenia = TxtContrasenia.Text;
            nuevo.Rol = BIZ.Modelo.Roles.Paciente;

            UsuarioDatos.Registrar(nuevo);

            Response.Redirect("~/Seguridad/Login.aspx");
        }
    }
}