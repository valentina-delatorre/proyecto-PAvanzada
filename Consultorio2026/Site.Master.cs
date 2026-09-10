using System;
using System.Web;

namespace Consultorio2026
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
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