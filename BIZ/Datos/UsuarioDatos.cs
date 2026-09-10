using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BIZ.Modelo;

namespace BIZ.Datos
{
    public class UsuarioDatos
    {
        public static Usuario ValidarLogin(string nombreUsuario, string contrasenia)
        {
            Usuario usuario = null;

            using (SqlConnection cn = Conexion.Obtener())
            {
                string sql = @"SELECT IdUsuario, NombreUsuario, Contrasenia, Rol, Activo
                               FROM Usuario
                               WHERE NombreUsuario = @NombreUsuario
                                 AND Contrasenia = @Contrasenia
                                 AND Activo = 1";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                cmd.Parameters.AddWithValue("@Contrasenia", contrasenia);

                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    usuario = new Usuario();
                    usuario.IdUsuario = (int)reader["IdUsuario"];
                    usuario.NombreUsuario = reader["NombreUsuario"].ToString();
                    usuario.Rol = reader["Rol"].ToString();
                    usuario.Activo = (bool)reader["Activo"];
                }
            }

            return usuario;
        }

        public static bool Existe(string nombreUsuario)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Usuario WHERE NombreUsuario = @NombreUsuario", cn);
                cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public static void Registrar(Usuario u)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Usuario (NombreUsuario, Contrasenia, Rol, Activo)
                      VALUES (@NombreUsuario, @Contrasenia, @Rol, 1)", cn);
                cmd.Parameters.AddWithValue("@NombreUsuario", u.NombreUsuario);
                cmd.Parameters.AddWithValue("@Contrasenia", u.Contrasenia);
                cmd.Parameters.AddWithValue("@Rol", u.Rol);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Todos los usuarios con sus datos "enriquecidos": ficha de paciente y/o de médico
        public static List<UsuarioListado> ListarTodos()
        {
            List<UsuarioListado> lista = new List<UsuarioListado>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                string sql = @"SELECT U.IdUsuario, U.NombreUsuario, U.Rol, U.Activo,
                                      ISNULL(D.Apellido + ', ' + D.Nombre, '-') AS NombreCompleto,
                                      ISNULL(D.Dni, '-') AS Dni,
                                      ISNULL(M.Especialidad, '-') AS Especialidad
                               FROM Usuario U
                               LEFT JOIN DatoPaciente D ON D.IdUsuario = U.IdUsuario
                               LEFT JOIN Medico M ON M.IdUsuario = U.IdUsuario
                               ORDER BY U.IdUsuario DESC";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UsuarioListado u = new UsuarioListado();
                    u.IdUsuario = (int)reader["IdUsuario"];
                    u.NombreUsuario = reader["NombreUsuario"].ToString();
                    u.Rol = reader["Rol"].ToString();
                    u.Activo = (bool)reader["Activo"];
                    u.NombreCompleto = reader["NombreCompleto"].ToString();
                    u.Dni = reader["Dni"].ToString();
                    u.Especialidad = reader["Especialidad"].ToString();
                    lista.Add(u);
                }
            }
            return lista;
        }

        public static void CambiarActivo(int idUsuario, bool activo)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuario SET Activo = @Activo WHERE IdUsuario = @IdUsuario", cn);
                cmd.Parameters.AddWithValue("@Activo", activo);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Usuarios con rol Medico que todavía no tienen ficha en la tabla Medico
        public static List<Usuario> ListarMedicosSinFicha()
        {
            List<Usuario> lista = new List<Usuario>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                string sql = @"SELECT U.IdUsuario, U.NombreUsuario
                               FROM Usuario U
                               LEFT JOIN Medico M ON M.IdUsuario = U.IdUsuario
                               WHERE U.Rol = 'Medico' AND M.IdMedico IS NULL AND U.Activo = 1";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Usuario u = new Usuario();
                    u.IdUsuario = (int)reader["IdUsuario"];
                    u.NombreUsuario = reader["NombreUsuario"].ToString();
                    lista.Add(u);
                }
            }
            return lista;
        }
    }
}