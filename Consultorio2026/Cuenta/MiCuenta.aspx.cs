using System;
using System.Web;
using System.Web.Security;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Cuenta
{
    public partial class MiCuenta : System.Web.UI.Page
    {
        private int IdUsuarioActual { get { return (int)Session["IdUsuario"]; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarDatos();
        }

        private void CargarDatos()
        {
            Usuario u = UsuarioDatos.ObtenerPorId(IdUsuarioActual);

            string extra = "";
            Medico medico = MedicoDatos.ObtenerPorUsuario(u.IdUsuario);
            if (medico != null)
                extra = " — Ficha profesional: Dr/a. " + medico.NombreCompleto + " (" + medico.Especialidad + ")";

            DatoPaciente datos = DatoPacienteDatos.Obtener(u.IdUsuario);
            if (datos != null)
                extra += " — Datos personales: " + datos.Apellido + ", " + datos.Nombre + " (DNI " + datos.Dni + ")";

            LbDatos.Text = "Usuario: <strong>" + u.NombreUsuario + "</strong> — Rol: " + u.Rol +
                           " — Estado: " + (u.Activo ? "Activo" : "Dado de baja") + extra;

            TxtEmail.Text = u.Email;
            TxtTelefono.Text = u.Telefono;
        }

        protected void BtGuardarContacto_Click(object sender, EventArgs e)
        {
            UsuarioDatos.ActualizarContacto(IdUsuarioActual, TxtEmail.Text.Trim(), TxtTelefono.Text.Trim());
            LbMsjContacto.Text = "✓ Datos de contacto guardados.";
        }

        protected void BtCambiarUsuario_Click(object sender, EventArgs e)
        {
            LbMsjUsuario.Text = ""; LbErrUsuario.Text = "";
            string nuevo = TxtNuevoUsuario.Text.Trim();

            if (nuevo == "") { LbErrUsuario.Text = "Escribí el nuevo nombre de usuario."; return; }
            if (!UsuarioDatos.ValidarContrasenia(IdUsuarioActual, TxtPassParaUsuario.Text))
            { LbErrUsuario.Text = "La contraseña actual no es correcta."; return; }
            if (UsuarioDatos.Existe(nuevo)) { LbErrUsuario.Text = "Ese nombre de usuario ya está en uso."; return; }

            UsuarioDatos.CambiarNombreUsuario(IdUsuarioActual, nuevo);

            // Renovamos la cookie y la Session para que el sistema te siga reconociendo con el nombre nuevo
            Usuario u = UsuarioDatos.ObtenerPorId(IdUsuarioActual);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, u.NombreUsuario, DateTime.Now, DateTime.Now.AddMinutes(30), false, u.Rol);
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));
            Session["NombreUsuario"] = u.NombreUsuario;

            LbMsjUsuario.Text = "✓ Nombre de usuario cambiado. A partir de ahora ingresás como \"" + nuevo + "\".";
            TxtNuevoUsuario.Text = ""; TxtPassParaUsuario.Text = "";
            CargarDatos();
        }

        protected void BtEnviarCodigo_Click(object sender, EventArgs e)
        {
            LbMsjPass.Text = ""; LbErrPass.Text = "";

            if (!UsuarioDatos.ValidarContrasenia(IdUsuarioActual, TxtPassActual.Text))
            { LbErrPass.Text = "La contraseña actual no es correcta."; return; }
            if (TxtPassNueva.Text.Length < 4)
            { LbErrPass.Text = "La nueva contraseña tiene que tener al menos 4 caracteres."; return; }
            if (TxtPassNueva.Text != TxtPassRepetir.Text)
            { LbErrPass.Text = "Las contraseñas nuevas no coinciden."; return; }

            Usuario u = UsuarioDatos.ObtenerPorId(IdUsuarioActual);
            if (u.Email == "" && u.Telefono == "")
            { LbErrPass.Text = "Primero cargá un email o teléfono en \"Datos de contacto\", para poder enviarte el código."; return; }

            string codigo = CodigoDatos.Generar(IdUsuarioActual);
            EnviarCodigo(u, codigo);

            // Guardamos la nueva contraseña "en espera" hasta que valide el código
            Session["PassPendiente"] = TxtPassNueva.Text;

            PnPaso1.Visible = false;
            PnPaso2.Visible = true;
        }

        private void EnviarCodigo(Usuario u, string codigo)
        {
            // ===== MODO DEMO: mostramos el código en pantalla =====
            LtCodigoDemo.Text = codigo;

            // ===== ENVÍO REAL POR MAIL (descomentar y configurar si la cátedra lo pide) =====
            // Requiere una cuenta de Gmail con "contraseña de aplicación" generada.
            //
            // var mensaje = new System.Net.Mail.MailMessage("tucuenta@gmail.com", u.Email,
            //     "Código de verificación - Consultorio",
            //     "Tu código es: " + codigo + " (vence en 10 minutos)");
            // var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
            // smtp.EnableSsl = true;
            // smtp.Credentials = new System.Net.NetworkCredential("tucuenta@gmail.com", "contraseña-de-aplicacion");
            // smtp.Send(mensaje);
        }

        protected void BtConfirmarCambio_Click(object sender, EventArgs e)
        {
            LbErrPass.Text = "";

            if (!CodigoDatos.Validar(IdUsuarioActual, TxtCodigo.Text.Trim()))
            { LbErrPass.Text = "Código incorrecto, vencido o ya usado. Volvé a empezar."; ResetearPasos(); return; }

            string nuevaPass = Session["PassPendiente"] as string;
            if (nuevaPass == null)
            { LbErrPass.Text = "La sesión del cambio expiró. Volvé a empezar."; ResetearPasos(); return; }

            UsuarioDatos.CambiarContrasenia(IdUsuarioActual, nuevaPass);
            Session.Remove("PassPendiente");

            ResetearPasos();
            LbMsjPass.Text = "✓ Contraseña cambiada correctamente. Usala en tu próximo inicio de sesión.";
        }

        protected void BtCancelarCambio_Click(object sender, EventArgs e)
        {
            Session.Remove("PassPendiente");
            ResetearPasos();
        }

        private void ResetearPasos()
        {
            PnPaso1.Visible = true;
            PnPaso2.Visible = false;
            TxtPassActual.Text = ""; TxtPassNueva.Text = ""; TxtPassRepetir.Text = ""; TxtCodigo.Text = "";
        }
    }
}