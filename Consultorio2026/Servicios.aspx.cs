using System;

namespace Consultorio2026
{
    public partial class Servicios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RpServicios.DataSource = ServiciosCatalogo.Todos();
                RpServicios.DataBind();
            }
        }
    }
}