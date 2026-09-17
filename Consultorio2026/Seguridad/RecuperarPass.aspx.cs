using System;
using BIZ;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Seguridad
{
    public partial class RecuperarPass : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void BtEnviarCodigo_Click(object sender, EventArgs e)
        {
            LbMsj.Text = ""; LbError.Text = "";
            string correo = TxtCorreo.Text.Trim();

            if (correo == "")
            {
                LbError.Text = "Escribí tu correo electrónico.";
                return;
            }

            Usuario u = UsuarioDatos.ObtenerPorCorreo(correo);

            // Si el correo no está registrado no lo decimos, para no andar
            // revelando qué cuentas existen. Igual pasamos al paso 2.
            if (u != null)
            {
                string codigo = CodigoDatos.Generar(u.IdUsuario);

                try
                {
                    Correo.EnviarCodigo(u.Email, codigo);
                }
                catch (Exception ex)
                {
                    LbError.Text = "No se pudo enviar el correo: " + ex.Message;
                    return;
                }

            }

            PnPaso1.Visible = false;
            PnPaso2.Visible = true;
            LbMsj.Text = "Si el correo está registrado, ya te llegó el código.";
        }

        protected void BtConfirmar_Click(object sender, EventArgs e)
        {
            LbMsj.Text = ""; LbError.Text = "";

            // No guardamos nada en la sesión: el que manda es el vencimiento del
            // código, que son 30 minutos. Al usuario lo volvemos a buscar por el
            // correo que escribió en el paso 1.
            Usuario u = UsuarioDatos.ObtenerPorCorreo(TxtCorreo.Text.Trim());

            if (u == null)
            {
                LbError.Text = "Código incorrecto, vencido o ya usado.";
                return;
            }

            int idUsuario = u.IdUsuario;

            if (TxtPassNueva.Text.Length < 4)
            {
                LbError.Text = "La nueva contraseña tiene que tener al menos 4 caracteres.";
                return;
            }

            if (TxtPassNueva.Text != TxtPassRepetir.Text)
            {
                LbError.Text = "Las contraseñas no coinciden.";
                return;
            }

            if (!CodigoDatos.Validar(idUsuario, TxtCodigo.Text.Trim()))
            {
                LbError.Text = "Código incorrecto, vencido o ya usado.";
                return;
            }

            UsuarioDatos.CambiarContrasenia(idUsuario, TxtPassNueva.Text);

            PnPaso2.Visible = false;
            LbMsj.Text = "✓ Listo. Ya podés iniciar sesión con tu contraseña nueva.";
        }

        protected void BtCancelar_Click(object sender, EventArgs e)
        {
            PnPaso2.Visible = false;
            PnPaso1.Visible = true;
            LbMsj.Text = ""; LbError.Text = "";
            TxtCodigo.Text = ""; TxtPassNueva.Text = ""; TxtPassRepetir.Text = "";
        }
    }
}
