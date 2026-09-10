using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BIZ.Modelo;

namespace BIZ.Datos
{
    public class TurnoDatos
    {
        public static void Solicitar(Turno t)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Turno (IdUsuarioPaciente, IdMedico, Especialidad, TipoConsulta, Fecha, Hora, Estado, Observaciones)
                      VALUES (@IdUsuarioPaciente, @IdMedico, @Especialidad, @TipoConsulta, @Fecha, @Hora, 'Solicitado', @Observaciones)", cn);
                cmd.Parameters.AddWithValue("@IdUsuarioPaciente", t.IdUsuarioPaciente);
                cmd.Parameters.AddWithValue("@IdMedico", t.IdMedico.HasValue ? (object)t.IdMedico.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Especialidad", t.Especialidad);
                cmd.Parameters.AddWithValue("@TipoConsulta", t.TipoConsulta);
                cmd.Parameters.AddWithValue("@Fecha", t.Fecha);
                cmd.Parameters.AddWithValue("@Hora", t.Hora);
                cmd.Parameters.AddWithValue("@Observaciones", (object)t.Observaciones ?? DBNull.Value);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static List<Turno> ListarSolicitados()
        {
            return ListarPorFiltro("WHERE T.Estado = 'Solicitado'", null, 0);
        }

        public static List<Turno> ListarPorPaciente(int idUsuarioPaciente)
        {
            return ListarPorFiltro("WHERE T.IdUsuarioPaciente = @Filtro", "@Filtro", idUsuarioPaciente);
        }

        public static List<Turno> ListarPorMedico(int idMedico)
        {
            return ListarPorFiltro("WHERE T.IdMedico = @Filtro AND T.Estado = 'Confirmado'", "@Filtro", idMedico);
        }

        // NUEVO: agenda completa de un médico, en cualquier estado
        public static List<Turno> ListarTodosPorMedico(int idMedico)
        {
            return ListarPorFiltro("WHERE T.IdMedico = @Filtro", "@Filtro", idMedico);
        }

        // NUEVO: historial completo del sistema
        public static List<Turno> ListarTodos()
        {
            return ListarPorFiltro("", null, 0);
        }

        // NUEVO: los turnos de hoy (para el reporte diario)
        public static List<Turno> ListarDelDia()
        {
            return ListarPorFiltro("WHERE T.Fecha = CAST(GETDATE() AS DATE)", null, 0);
        }

        private static List<Turno> ListarPorFiltro(string where, string nombreParam, int valorParam)
        {
            List<Turno> lista = new List<Turno>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                string sql = @"SELECT T.IdTurno, T.IdUsuarioPaciente, T.IdMedico, T.Especialidad,
                                      T.TipoConsulta, T.Fecha, T.Hora, T.Estado, T.Observaciones,
                                      U.NombreUsuario AS NombrePaciente,
                                      ISNULL(M.Apellido + ', ' + M.Nombre, '-') AS NombreMedico
                               FROM Turno T
                               INNER JOIN Usuario U ON U.IdUsuario = T.IdUsuarioPaciente
                               LEFT JOIN Medico M ON M.IdMedico = T.IdMedico "
                               + where + " ORDER BY T.Fecha, T.Hora";

                SqlCommand cmd = new SqlCommand(sql, cn);
                if (nombreParam != null)
                    cmd.Parameters.AddWithValue(nombreParam, valorParam);

                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Turno t = new Turno();
                    t.IdTurno = (int)reader["IdTurno"];
                    t.IdUsuarioPaciente = (int)reader["IdUsuarioPaciente"];
                    t.IdMedico = reader["IdMedico"] == DBNull.Value ? (int?)null : (int)reader["IdMedico"];
                    t.Especialidad = reader["Especialidad"].ToString();
                    t.TipoConsulta = reader["TipoConsulta"].ToString();
                    t.Fecha = (DateTime)reader["Fecha"];
                    t.Hora = reader["Hora"].ToString();
                    t.Estado = reader["Estado"].ToString();
                    t.Observaciones = reader["Observaciones"] == DBNull.Value ? "" : reader["Observaciones"].ToString();
                    t.NombrePaciente = reader["NombrePaciente"].ToString();
                    t.NombreMedico = reader["NombreMedico"].ToString();
                    lista.Add(t);
                }
            }
            return lista;
        }

        public static void Resolver(int idTurno, int? idMedico, string nuevoEstado)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Turno SET Estado = @Estado,
                             IdMedico = ISNULL(@IdMedico, IdMedico)
                      WHERE IdTurno = @IdTurno", cn);
                cmd.Parameters.AddWithValue("@Estado", nuevoEstado);
                cmd.Parameters.AddWithValue("@IdMedico", idMedico.HasValue ? (object)idMedico.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@IdTurno", idTurno);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        // Solicitudes que el paciente hizo eligiendo a ESTE médico, pendientes de su decisión
        public static List<Turno> ListarPendientesPorMedico(int idMedico)
        {
            return ListarPorFiltro("WHERE T.IdMedico = @Filtro AND T.Estado = 'Solicitado'", "@Filtro", idMedico);
        }
    }
}