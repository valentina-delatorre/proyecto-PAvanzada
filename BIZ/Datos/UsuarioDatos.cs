using System;
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
    }
}