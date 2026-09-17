using System.Net.Mail;

namespace BIZ
{
    // El servidor de salida (host, usuario y contraseña) se configura en el
    // Web.config, dentro de <system.net><mailSettings>. SmtpClient lo lee solo,
    // así que acá no hay ninguna credencial escrita.
    public static class Correo
    {
        public static void EnviarCodigo(string destino, string codigo)
        {
            string cuerpo =
                "Tu código de verificación es: " + codigo + "\r\n\r\n" +
                "Vence en 30 minutos y se puede usar una sola vez.\r\n" +
                "Si no pediste este código, ignorá este mensaje.";

            using (MailMessage mensaje = new MailMessage())
            {
                mensaje.To.Add(destino);
                mensaje.Subject = "Código de verificación - Consultorio";
                mensaje.Body = cuerpo;

                new SmtpClient().Send(mensaje);
            }
        }
    }
}
