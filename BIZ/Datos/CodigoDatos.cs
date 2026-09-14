using System;
using System.Data.SqlClient;

namespace BIZ.Datos
{
    public class CodigoDatos
    {
        // Genera un código de 6 dígitos, invalida los anteriores, y lo guarda con vencimiento a 10 minutos
        public static string Generar(int idUsuario)
        {
            string codigo = new Random().Next(100000, 999999).ToString();

            using (SqlConnection cn = Conexion.Obtener())
            {
                cn.Open();

                SqlCommand invalidar = new SqlCommand(
                    "UPDATE CodigoVerificacion SET Usado = 1 WHERE IdUsuario = @IdUsuario", cn);
                invalidar.Parameters.AddWithValue("@IdUsuario", idUsuario);
                invalidar.ExecuteNonQuery();

                SqlCommand insertar = new SqlCommand(
                    @"INSERT INTO CodigoVerificacion (IdUsuario, Codigo, FechaExpira, Usado)
                      VALUES (@IdUsuario, @Codigo, DATEADD(MINUTE, 10, GETDATE()), 0)", cn);
                insertar.Parameters.AddWithValue("@IdUsuario", idUsuario);
                insertar.Parameters.AddWithValue("@Codigo", codigo);
                insertar.ExecuteNonQuery();
            }

            return codigo;
        }

        // Valida y "quema" el código: solo sirve una vez, y solo dentro de los 10 minutos
        public static bool Validar(int idUsuario, string codigo)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(
                    @"SELECT IdCodigo FROM CodigoVerificacion
                      WHERE IdUsuario = @IdUsuario AND Codigo = @Codigo
                        AND Usado = 0 AND FechaExpira > GETDATE()", cn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@Codigo", codigo);

                object resultado = cmd.ExecuteScalar();
                if (resultado == null) return false;

                SqlCommand quemar = new SqlCommand(
                    "UPDATE CodigoVerificacion SET Usado = 1 WHERE IdCodigo = @IdCodigo", cn);
                quemar.Parameters.AddWithValue("@IdCodigo", (int)resultado);
                quemar.ExecuteNonQuery();

                return true;
            }
        }
    }
}