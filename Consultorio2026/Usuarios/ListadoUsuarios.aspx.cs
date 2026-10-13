using System;
using System.Collections.Generic;
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
            GvUsuarios.DataSource = UsuarioDatos.ListarTodos();
            GvUsuarios.DataBind();
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
            LbDetalleUsuario.Text = "Usuario: <strong>" + u.NombreUsuario + "</strong> — Rol: " + u.Rol +
                                    " — Nombre: " + u.NombreCompleto + " — DNI: " + u.Dni +
                                    (u.Especialidad != "-" ? " — Especialidad: " + u.Especialidad : "") +
                                    " — Estado: " + (u.Activo ? "Activo" : "Dado de baja");

            // Si es paciente, mostramos la ficha completa que cargó en el portal
            DatoPaciente ficha = DatoPacienteDatos.Obtener(u.IdUsuario);
            LbFichaPaciente.Text = (u.Rol == Roles.Paciente && ficha != null)
                ? "Teléfono: " + (ficha.Telefono != "" ? ficha.Telefono : "-") +
                  " — Correo: " + (ficha.Email != "" ? ficha.Email : "-")
                : "";

            // El administrador (Seguridad) es el único usuario que no se puede dar de baja
            bool esAdmin = (u.Rol == Roles.Seguridad);
            BtCambiarActivo.Visible = !esAdmin;
            LbProtegido.Text = esAdmin ? "El usuario administrador no se puede dar de baja." : "";
            BtCambiarActivo.Text = u.Activo ? "Dar de baja este usuario" : "Reactivar este usuario";

            List<Turno> turnos = TurnoDatos.ListarPorPaciente(u.IdUsuario);
            LbTotalTurnos.Text = turnos.Count + " turno(s) desde que se creó la cuenta.";
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
