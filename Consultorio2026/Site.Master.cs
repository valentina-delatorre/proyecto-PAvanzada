using System;
using System.Web.Security;

namespace Consultorio2026
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Rol"] != null)
            {
                string rol = Session["Rol"].ToString();
                LbUsuarioLogueado.Text = "Usuario: " + Session["NombreUsuario"] + " (" + rol + ")";

                if (rol == BIZ.Modelo.Roles.Seguridad)
                    PnMenuSeguridad.Visible = true;
                else if (rol == BIZ.Modelo.Roles.Usuarios)
                    PnMenuUsuarios.Visible = true;
                else if (rol == BIZ.Modelo.Roles.Especialistas)
                    PnMenuEspecialistas.Visible = true;
                else if (rol == BIZ.Modelo.Roles.Reportes)
                    PnMenuReportes.Visible = true;
            }
        }

        protected void BtCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Seguridad/Login.aspx");
        }
       
    }
}