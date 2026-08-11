using System;

namespace Consultorio2026
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Rol"] != null)
            {
                string rol = Session["Rol"].ToString();

                LbUsuarioLogueado.Text = "Usuario: " + Session["NombreUsuario"].ToString() + " (" + rol + ")";

                if (rol == BIZ.Modelo.Roles.Administrador)
                    PnMenuAdmin.Visible = true;
                else if (rol == BIZ.Modelo.Roles.Especialista)
                    PnMenuEspecialista.Visible = true;
                else if (rol == BIZ.Modelo.Roles.Recepcionista)
                    PnMenuRecepcionista.Visible = true;
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