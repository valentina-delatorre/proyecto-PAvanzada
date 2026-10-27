using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Usuarios
{
    public partial class ListadoUsuarios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            List<UsuarioListado> usuarios = UsuarioDatos.ListarTodos();
            GvUsuarios.DataSource = usuarios;
            GvUsuarios.DataBind();

            // Los numeros de la cabecera
            LtTotal.Text = usuarios.Count.ToString();
            LtPacientes.Text = usuarios.Count(u => u.Rol == Roles.Paciente).ToString();
            LtBajas.Text = usuarios.Count(u => !u.Activo).ToString();
        }

        // Nombre y apellido reales de cada usuario (de la tabla PERSONA), se trae una vez por pedido
        private Dictionary<int, string> nombres;
        protected string Nombre(object idUsuario)
        {
            if (nombres == null) nombres = UsuarioDatos.NombresReales();
            string nombre;
            return HttpUtility.HtmlEncode(nombres.TryGetValue((int)idUsuario, out nombre) ? nombre : "-");
        }

        private UsuarioListado UsuarioSeleccionado()
        {
            int id = (int)GvUsuarios.SelectedDataKey.Value;
            return UsuarioDatos.ListarTodos().Find(u => u.IdUsuario == id);
        }

        protected void GvUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            MostrarDetalle();
        }

        private void MostrarDetalle()
        {
            UsuarioListado u = UsuarioSeleccionado();
            if (u == null) { PnDetalle.Visible = false; return; }

            PnDetalle.Visible = true;
            LtDetalleNombre.Text = Nombre(u.IdUsuario);

            // La ficha: cada dato en su casillero. Si es paciente, sumamos lo que cargo en el portal.
            DatoPaciente ficha = DatoPacienteDatos.Obtener(u.IdUsuario);
            var datos = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Usuario", u.NombreUsuario),
                new KeyValuePair<string, string>("Rol", u.Rol),
                new KeyValuePair<string, string>("Correo", u.Email),
                new KeyValuePair<string, string>("DNI", u.Dni),
                new KeyValuePair<string, string>("Estado", u.Activo ? "Activo" : "Dado de baja")
            };
            if (u.Especialidad != "-")
                datos.Add(new KeyValuePair<string, string>("Especialidad", u.Especialidad));
            if (u.Rol == Roles.Paciente && ficha != null)
                datos.Add(new KeyValuePair<string, string>("Teléfono", ficha.Telefono != "" ? ficha.Telefono : "-"));

            LtFicha.Text = "<dl class=\"ficha\">" + string.Concat(datos.Select(d =>
                "<div><dt>" + d.Key + "</dt><dd>" + HttpUtility.HtmlEncode(d.Value) + "</dd></div>")) + "</dl>";

            // El administrador (Seguridad) es el único usuario que no se puede dar de baja
            bool esAdmin = (u.Rol == Roles.Seguridad);
            BtCambiarActivo.Visible = !esAdmin;
            LbProtegido.Text = esAdmin ? "El usuario administrador no se puede dar de baja." : "";
            BtCambiarActivo.Text = u.Activo ? "Dar de baja este usuario" : "Reactivar este usuario";
            BtCambiarActivo.CssClass = u.Activo ? "btn btn-mini btn-peligro" : "btn btn-mini";

            List<Turno> turnos = TurnoDatos.ListarPorPaciente(u.IdUsuario);
            LbTotalTurnos.Text = turnos.Count == 0 ? "" : turnos.Count + (turnos.Count == 1 ? " turno" : " turnos") + " desde que se creó la cuenta.";
            GvTurnosUsuario.DataSource = turnos;
            GvTurnosUsuario.DataBind();
        }

        protected void BtCambiarActivo_Click(object sender, EventArgs e)
        {
            UsuarioListado u = UsuarioSeleccionado();
            if (u == null || u.Rol == Roles.Seguridad) return;

            UsuarioDatos.CambiarActivo(u.IdUsuario, !u.Activo);
            CargarUsuarios();
            MostrarDetalle();
        }
    }
}
