using System.Configuration;
using System.Data.SqlClient;

namespace BIZ.Datos
{
    public class Conexion
    {
        public static SqlConnection Obtener()
        {
            string cadena = ConfigurationManager
                .ConnectionStrings["ConsultorioDB"].ConnectionString;
            return new SqlConnection(cadena);
        }
    }
}