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
            string correo = TxtCorreo.Text.Trim();

            if (correo == "" || TxtContrasenia.Text == "")
            {
                LbError.Text = "Completá correo y contraseña.";
                return;
            }

            if (!EsCorreoValido(correo))
            {
                LbError.Text = "Escribí un correo electrónico válido.";
                return;
            }

            if (TxtContrasenia.Text != TxtConfirmar.Text)
            {
                LbError.Text = "Las contraseñas no coinciden.";
                return;
            }

            if (UsuarioDatos.ExisteCorreo(correo))
            {
                LbError.Text = "Ya hay una cuenta registrada con ese correo.";
                return;
            }

            Usuario nuevo = new Usuario();
            nuevo.Email = correo;
            nuevo.NombreUsuario = NombreParaMostrar(correo);
            nuevo.Contrasenia = TxtContrasenia.Text;
            nuevo.Rol = BIZ.Modelo.Roles.Paciente;

            UsuarioDatos.Registrar(nuevo);

            Response.Redirect("~/Seguridad/Login.aspx");
        }

        private static bool EsCorreoValido(string correo)
        {
            try { return new System.Net.Mail.MailAddress(correo).Address == correo; }
            catch { return false; }
        }

        // Se entra con el correo, pero arriba a la derecha hay que mostrar algo:
        // usamos lo que va antes de la arroba, y si ya está tomado le sumamos un número.
        private static string NombreParaMostrar(string correo)
        {
            string baseNombre = correo.Split('@')[0];
            string nombre = baseNombre;
            int n = 2;

            while (UsuarioDatos.Existe(nombre))
            {
                nombre = baseNombre + n;
                n++;
            }

            return nombre;
        }
    }
}
