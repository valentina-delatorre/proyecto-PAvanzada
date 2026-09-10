using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BIZ.Modelo;

namespace BIZ.Datos
{
    public class MedicoDatos
    {
        public static List<Medico> ListarTodos()
        {
            List<Medico> lista = new List<Medico>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                string sql = @"SELECT M.IdMedico, M.IdUsuario, M.Nombre, M.Apellido, M.Especialidad, M.Activo,
                                      ISNULL(U.NombreUsuario, '-') AS UsuarioVinculado
                               FROM Medico M
                               LEFT JOIN Usuario U ON U.IdUsuario = M.IdUsuario
                               ORDER BY M.Apellido";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
            }
            return lista;
        }

        public static List<Medico> ListarPorEspecialidad(string especialidad)
        {
            List<Medico> lista = new List<Medico>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT IdMedico, IdUsuario, Nombre, Apellido, Especialidad, Activo
                      FROM Medico WHERE Activo = 1 AND Especialidad = @Especialidad
                      ORDER BY Apellido", cn);
                cmd.Parameters.AddWithValue("@Especialidad", especialidad);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
            }
            return lista;
        }

        public static List<string> ListarEspecialidades()
        {
            List<string> lista = new List<string>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT DISTINCT Especialidad FROM Medico WHERE Activo = 1 ORDER BY Especialidad", cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(reader["Especialidad"].ToString());
            }
            return lista;
        }

        public static Medico ObtenerPorUsuario(int idUsuario)
        {
            Medico m = null;
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT IdMedico, IdUsuario, Nombre, Apellido, Especialidad, Activo
                      FROM Medico WHERE IdUsuario = @IdUsuario AND Activo = 1", cn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                    m = Mapear(reader);
            }
            return m;
        }

        public static void Insertar(Medico m)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Medico (IdUsuario, Nombre, Apellido, Especialidad, Activo)
                      VALUES (@IdUsuario, @Nombre, @Apellido, @Especialidad, 1)", cn);
                cmd.Parameters.AddWithValue("@IdUsuario", m.IdUsuario == 0 ? (object)DBNull.Value : m.IdUsuario);
                cmd.Parameters.AddWithValue("@Nombre", m.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", m.Apellido);
                cmd.Parameters.AddWithValue("@Especialidad", m.Especialidad);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void CambiarActivo(int idMedico, bool activo)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Medico SET Activo = @Activo WHERE IdMedico = @IdMedico", cn);
                cmd.Parameters.AddWithValue("@Activo", activo);
                cmd.Parameters.AddWithValue("@IdMedico", idMedico);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static Medico Mapear(SqlDataReader reader)
        {
            Medico m = new Medico();
            m.IdMedico = (int)reader["IdMedico"];
            m.IdUsuario = reader["IdUsuario"] == DBNull.Value ? 0 : (int)reader["IdUsuario"];
            m.Nombre = reader["Nombre"].ToString();
            m.Apellido = reader["Apellido"].ToString();
            m.Especialidad = reader["Especialidad"].ToString();
            m.Activo = (bool)reader["Activo"];

            // La columna UsuarioVinculado solo viene en ListarTodos
            bool tieneColumna = false;
            for (int i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i) == "UsuarioVinculado") { tieneColumna = true; break; }
            m.UsuarioVinculado = tieneColumna ? reader["UsuarioVinculado"].ToString() : "";

            return m;
        }
    }
}