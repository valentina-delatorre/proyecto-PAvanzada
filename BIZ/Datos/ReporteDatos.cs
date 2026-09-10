using System.Collections.Generic;
using System.Data.SqlClient;
using BIZ.Modelo;

namespace BIZ.Datos
{
    public class ReporteDatos
    {
        public static List<ResumenFila> TotalesPorEstado()
        {
            return Agrupar("SELECT Estado AS Etiqueta, COUNT(*) AS Cantidad FROM Turno GROUP BY Estado");
        }

        public static List<ResumenFila> TurnosPorEspecialidad()
        {
            return Agrupar("SELECT Especialidad AS Etiqueta, COUNT(*) AS Cantidad FROM Turno GROUP BY Especialidad ORDER BY COUNT(*) DESC");
        }

        public static List<ResumenFila> ProximosPorMedico()
        {
            return Agrupar(@"SELECT ISNULL(M.Apellido + ', ' + M.Nombre, 'Sin asignar') AS Etiqueta, COUNT(*) AS Cantidad
                             FROM Turno T
                             LEFT JOIN Medico M ON M.IdMedico = T.IdMedico
                             WHERE T.Estado = 'Confirmado' AND T.Fecha >= CAST(GETDATE() AS DATE)
                             GROUP BY ISNULL(M.Apellido + ', ' + M.Nombre, 'Sin asignar')
                             ORDER BY COUNT(*) DESC");
        }

        private static List<ResumenFila> Agrupar(string sql)
        {
            List<ResumenFila> lista = new List<ResumenFila>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ResumenFila f = new ResumenFila();
                    f.Etiqueta = reader["Etiqueta"].ToString();
                    f.Cantidad = (int)reader["Cantidad"];
                    lista.Add(f);
                }
            }
            return lista;
        }
    }
}