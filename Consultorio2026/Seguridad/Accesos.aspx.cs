using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using BIZ.Datos;
using BIZ.Modelo;

/*
   SEGURIDAD Y ACCESOS (solo la ve Valentina)

   Una tabla con todos los usuarios y una columna por modulo. Cada casillero dice si
   esa persona puede entrar a ese modulo. Al tocarlo se guarda enseguida (AutoPostBack).

   De donde sale cada casillero: la vista VW_ACCESOS de la base. Por defecto cada uno
   tiene lo que le da su rol (ROL_PERMISO); si aca se lo cambia, queda guardada la
   excepcion en USUARIO_PERMISO. El que despues frena o deja pasar a la persona es
   Global.asax (Application_PostAcquireRequestState).

   El modulo "Seguridad y Accesos" no aparece en la tabla: es solo de Valentina y
   no se puede regalar ni quitar desde aca.
*/

namespace Consultorio2026.Seguridad
{
    public partial class Accesos : System.Web.UI.Page
    {
        // Nombre corto de cada modulo para el encabezado de la tabla
        private static readonly Dictionary<string, string> Cortos = new Dictionary<string, string>
        {
            { "ABM Usuarios",      "Usuarios" },
            { "ABM Especialistas", "Especialistas" },
            { "Gestion de Turnos", "Turnos" },
            { "Ver Reportes",      "Reportes" },
            { "Panel Medico",      "Panel médico" },
            { "Portal Paciente",   "Portal paciente" },
            { "Asistente IA",      "Asistente IA" }
        };

        // Todos los accesos de todos los usuarios; se trae una sola vez por carga
        private List<Acceso> todos;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                Cargar();
        }

        private void Cargar()
        {
            todos = AccesoDatos.ListarTodos().Where(a => a.Modulo != AccesoDatos.ModuloAccesos).ToList();

            // Las columnas: los modulos que tiene el primer usuario (todos tienen los mismos)
            RpModulos.DataSource = todos.Select(a => a.Modulo).Distinct()
                .Select(m => new { Corto = Cortos.ContainsKey(m) ? Cortos[m] : m });
            RpModulos.DataBind();

            List<UsuarioListado> usuarios = UsuarioDatos.ListarTodos();
            RpUsuarios.DataSource = usuarios.OrderBy(u => u.Rol).ThenBy(u => u.NombreUsuario);
            RpUsuarios.DataBind();

            // Los numeros de la cabecera
            LtUsuarios.Text = usuarios.Count.ToString();
            LtModulos.Text = todos.Select(a => a.IdPermiso).Distinct().Count().ToString();
            LtCambios.Text = todos.Count(a => a.Habilitado != a.PorRol).ToString();
        }

        // Nombre y apellido reales (de la tabla PERSONA), se trae una vez por pedido
        private Dictionary<int, string> nombres;
        protected string Nombre(object idUsuario)
        {
            if (nombres == null) nombres = UsuarioDatos.NombresReales();
            string nombre;
            return System.Web.HttpUtility.HtmlEncode(nombres.TryGetValue((int)idUsuario, out nombre) ? nombre : "-");
        }

        // Por cada usuario, le cargamos sus casilleros
        protected void RpUsuarios_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            int idUsuario = ((UsuarioListado)e.Item.DataItem).IdUsuario;
            Repeater rp = (Repeater)e.Item.FindControl("RpAccesos");
            rp.DataSource = todos.Where(a => a.IdUsuario == idUsuario);
            rp.DataBind();
        }

        // Tocaron un casillero: el usuario y el modulo salen de los campos ocultos de esa fila y esa celda
        protected void ChkAcceso_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            RepeaterItem celda = (RepeaterItem)chk.NamingContainer;
            RepeaterItem fila = (RepeaterItem)celda.NamingContainer.NamingContainer;

            int idUsuario = int.Parse(((HiddenField)fila.FindControl("HfUsuario")).Value);
            int idPermiso = int.Parse(((HiddenField)celda.FindControl("HfPermiso")).Value);

            AccesoDatos.Cambiar(idUsuario, idPermiso, chk.Checked);

            LbMensaje.Text = "✓ " + (chk.Checked ? "Habilitado" : "Deshabilitado") + ": " + chk.ToolTip
                + ". Corre desde la próxima página que abra esa persona.";
            Cargar();   // redibuja para que se actualice la marca de "cambiado a mano"
        }
    }
}
