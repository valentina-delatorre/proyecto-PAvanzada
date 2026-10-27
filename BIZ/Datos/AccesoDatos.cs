using System.Collections.Generic;
using System.Data.SqlClient;
using BIZ.Modelo;

namespace BIZ.Datos
{
    // Accesos de cada usuario a cada modulo. La cuenta "rol + excepciones"
    // la hace la vista VW_ACCESOS (BaseDatos/08_accesos.sql).
    public class AccesoDatos
    {
        public const string ModuloAccesos = "Seguridad y Accesos";
        public const string ModuloAsistente = "Asistente IA";

        // Todos los modulos con el si/no de cada usuario, para la pantalla de Accesos
        public static List<Acceso> ListarTodos()
        {
            return Listar("", 0);
        }

        // Los modulos a los que entra un usuario (solo los habilitados)
        public static List<string> ModulosHabilitados(int idUsuario)
        {
            List<string> modulos = new List<string>();
            foreach (Acceso a in Listar("AND ID_USUARIO = @IdUsuario", idUsuario))
                if (a.Habilitado) modulos.Add(a.Modulo);
            return modulos;
        }

        // ¿Este usuario puede abrir esta pagina?
        // Si la pagina no figura en la tabla URL, no es de ningun modulo y puede entrar cualquiera logueado.
        public static bool PuedeEntrar(int idUsuario, string ruta)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(*) AS MODULOS,
                             ISNULL(SUM(CASE WHEN A.HABILITADO = 1 THEN 1 ELSE 0 END), 0) AS HABILITADOS
                      FROM URL U
                      JOIN VW_ACCESOS A ON A.ID_PERMISO = U.ID_PERMISO AND A.ID_USUARIO = @IdUsuario
                      WHERE U.RUTA = @Ruta AND U.ESTADO = 1", cn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@Ruta", ruta);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return (int)reader["MODULOS"] == 0 || (int)reader["HABILITADOS"] > 0;
            }
        }

        // Habilita o deshabilita un modulo a un usuario.
        // Si queda igual que lo que le da su rol, se borra la excepcion: no hace falta guardarla.
        public static void Cambiar(int idUsuario, int idPermiso, bool habilitado)
        {
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"DELETE FROM USUARIO_PERMISO WHERE ID_USUARIO = @IdUsuario AND ID_PERMISO = @IdPermiso;

                      INSERT INTO USUARIO_PERMISO (ID_USUARIO, ID_PERMISO, HABILITADO)
                      SELECT @IdUsuario, @IdPermiso, @Habilitado
                      FROM VW_ACCESOS
                      WHERE ID_USUARIO = @IdUsuario AND ID_PERMISO = @IdPermiso AND POR_ROL <> @Habilitado;", cn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@IdPermiso", idPermiso);
                cmd.Parameters.AddWithValue("@Habilitado", habilitado);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static List<Acceso> Listar(string filtro, int idUsuario)
        {
            List<Acceso> lista = new List<Acceso>();
            using (SqlConnection cn = Conexion.Obtener())
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT ID_USUARIO, ID_PERMISO, NOMBRE_PERMISO, HABILITADO, POR_ROL
                      FROM VW_ACCESOS WHERE 1 = 1 " + filtro + " ORDER BY ID_USUARIO, ID_PERMISO", cn);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Acceso a = new Acceso();
                    a.IdUsuario = (int)reader["ID_USUARIO"];
                    a.IdPermiso = (int)reader["ID_PERMISO"];
                    a.Modulo = reader["NOMBRE_PERMISO"].ToString();
                    a.Habilitado = (bool)reader["HABILITADO"];
                    a.PorRol = (bool)reader["POR_ROL"];
                    lista.Add(a);
                }
            }
            return lista;
        }
    }
}
