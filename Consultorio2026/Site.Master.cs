using System;
using System.Web;

namespace Consultorio2026
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Esto corre ANTES que el Page_Load de cualquier página (Init va antes que Load),
            // por eso sirve de "portero" para todas las pantallas que usan este master.
            //
            // El problema que evita: el login se recuerda de dos formas distintas.
            //   - La cookie de login vive en el NAVEGADOR y dura 30 minutos.
            //   - La sesión (Session["IdUsuario"], el rol, etc.) vive en la MEMORIA DEL SERVIDOR.
            // Si se reinicia la app (por ejemplo al recompilar) la memoria se borra, pero la cookie sigue.
            // Entonces la app te deja pasar sin saber quién sos, y al buscar Session["IdUsuario"] explota.
            // Si pasa eso, cerramos el login y lo mandamos a entrar de nuevo.
            if (Request.IsAuthenticated && Session["IdUsuario"] == null)
            {
                System.Web.Security.FormsAuthentication.SignOut();
                Response.Redirect("~/Seguridad/Login.aspx");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Rol"] != null)
            {
                string rol = Session["Rol"].ToString();
                string nombre = Session["NombreUsuario"] != null ? Session["NombreUsuario"].ToString() : "";

                LbUsuarioLogueado.Text = nombre + " · " + rol;

                string rolSeguro = HttpUtility.JavaScriptStringEncode(rol);
                LtRolScript.Text = "<script>var rolActual = '" + rolSeguro + "';</script>";
            }
        }

        protected void BtCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            System.Web.Security.FormsAuthentication.SignOut();
            Response.Redirect("~/Seguridad/Login.aspx");
        }
    }
}