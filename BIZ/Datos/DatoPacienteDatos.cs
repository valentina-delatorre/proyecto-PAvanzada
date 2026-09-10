using System;
using System.Data.SqlClient;
using BIZ.Modelo;

namespace BIZ.Datos
{
    public class DatoPacienteDatos
    {
        public static DatoPaciente Obtener(int idUsuario)
        {
            DatoPaciente d = null;
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT IdDatoPaciente, IdUsuario, Nombre, Apellido, Dni, Telefono, Email
                      FROM DatoPaciente WHERE IdUsuario = @IdUsuario", cn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    d = new DatoPaciente();
                    d.IdDatoPaciente = (int)reader["IdDatoPaciente"];
                    d.IdUsuario = (int)reader["IdUsuario"];
                    d.Nombre = reader["Nombre"].ToString();
                    d.Apellido = reader["Apellido"].ToString();
                    d.Dni = reader["Dni"].ToString();
                    d.Telefono = reader["Telefono"] == DBNull.Value ? "" : reader["Telefono"].ToString();
                    d.Email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString();
                }
            }
            return d;
        }

        public static void Guardar(DatoPaciente d)
        {
            bool existe = Obtener(d.IdUsuario) != null;

            using (SqlConnection cn = Conexion.Obtener())
            {
                string sql = existe
                    ? @"UPDATE DatoPaciente SET Nombre=@Nombre, Apellido=@Apellido, Dni=@Dni,
                               Telefono=@Telefono, Email=@Email WHERE IdUsuario=@IdUsuario"
                    : @"INSERT INTO DatoPaciente (IdUsuario, Nombre, Apellido, Dni, Telefono, Email)
                        VALUES (@IdUsuario, @Nombre, @Apellido, @Dni, @Telefono, @Email)";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@IdUsuario", d.IdUsuario);
                cmd.Parameters.AddWithValue("@Nombre", d.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", d.Apellido);
                cmd.Parameters.AddWithValue("@Dni", d.Dni);
                cmd.Parameters.AddWithValue("@Telefono", (object)d.Telefono ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)d.Email ?? DBNull.Value);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}