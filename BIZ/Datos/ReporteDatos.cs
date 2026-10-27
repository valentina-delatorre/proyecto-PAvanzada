using System.Collections.Generic;
using System.Data.SqlClient;
using BIZ.Modelo;

namespace BIZ.Datos
{
    // =====================================================================
    // CLASE ReporteDatos - Capa de acceso a datos del módulo Reportes
    // =====================================================================
    // Esta clase está en el proyecto BIZ, que es la capa de negocio y datos.
    // Su única responsabilidad es consultar la base de datos y devolver los
    // resultados ya armados como objetos de C#.
    //
    // La página de reportes (ReporteDiario.aspx.cs) nunca escribe SQL: solo
    // llama a los métodos de esta clase. De esta forma separamos la pantalla
    // del acceso a datos, como trabajamos en todo el proyecto.
    //
    // Todos los reportes devuelven una lista de ResumenFila. ResumenFila es
    // una clase muy simple que tiene solo dos datos:
    //   - Etiqueta: el nombre de lo que se cuenta (un estado, una especialidad o un médico).
    //   - Cantidad: cuántos turnos corresponden a esa etiqueta.
    // =====================================================================
    public class ReporteDatos
    {
        // ---------------------------------------------------------------
        // Reporte "Totales por estado"
        // ---------------------------------------------------------------
        // Devuelve cuántos turnos hay en cada estado, por ejemplo:
        //   Confirmado -> 12
        //   Cancelado  -> 3
        //
        // Cómo funciona la consulta:
        //   - FROM Turno: leemos la tabla de turnos.
        //   - GROUP BY Estado: juntamos en un mismo grupo todos los turnos
        //     que tienen el mismo estado.
        //   - COUNT(*): cuenta cuántos turnos quedaron en cada grupo.
        //   - "AS Etiqueta" y "AS Cantidad": les ponemos esos nombres a las
        //     columnas para que el método Agrupar las pueda leer siempre igual.
        public static List<ResumenFila> TotalesPorEstado()
        {
            return Agrupar("SELECT Estado AS Etiqueta, COUNT(*) AS Cantidad FROM Turno GROUP BY Estado");
        }

        // ---------------------------------------------------------------
        // Reporte "Turnos por especialidad"
        // ---------------------------------------------------------------
        // Devuelve cuántos turnos tiene cada especialidad, por ejemplo:
        //   Cardiologia -> 8
        //   Pediatria   -> 5
        //
        // Es la misma idea que el reporte anterior, pero agrupando por
        // especialidad en lugar de por estado.
        // ORDER BY COUNT(*) DESC ordena el resultado de mayor a menor, así
        // la especialidad más pedida aparece primera.
        public static List<ResumenFila> TurnosPorEspecialidad()
        {
            return Agrupar("SELECT Especialidad AS Etiqueta, COUNT(*) AS Cantidad FROM Turno GROUP BY Especialidad ORDER BY COUNT(*) DESC");
        }

        // ---------------------------------------------------------------
        // Reporte "Próximos turnos por especialista"
        // ---------------------------------------------------------------
        // Devuelve cuántos turnos confirmados tiene cada médico desde hoy en
        // adelante. Sirve para ver la carga de trabajo que viene.
        //
        // Cómo funciona la consulta:
        //   - FROM Turno T: leemos los turnos ("T" es un alias, un nombre corto
        //     para no escribir "Turno" cada vez).
        //   - LEFT JOIN Medico M: unimos cada turno con los datos de su médico
        //     para poder mostrar el nombre. Usamos LEFT JOIN y no un JOIN común
        //     porque, si un turno todavía no tiene médico asignado, con un JOIN
        //     común ese turno desaparecería del resultado. Con LEFT JOIN lo
        //     seguimos contando.
        //   - ISNULL(..., 'Sin asignar'): si el turno no tiene médico, el nombre
        //     queda vacío (NULL); en ese caso mostramos "Sin asignar".
        //   - WHERE T.Estado = 'Confirmado': solo contamos los turnos confirmados,
        //     no los cancelados.
        //   - T.Fecha >= CAST(GETDATE() AS DATE): solo los turnos de hoy en adelante.
        //     GETDATE() devuelve la fecha y la hora actual; con CAST(... AS DATE)
        //     le sacamos la hora, para que los turnos de hoy también entren.
        //   - GROUP BY: agrupamos por el nombre del médico. Se repite la misma
        //     expresión del SELECT porque SQL Server no permite agrupar usando
        //     el alias "Etiqueta".
        //   - ORDER BY COUNT(*) DESC: el médico con más turnos aparece primero.
        public static List<ResumenFila> ProximosPorMedico()
        {
            return Agrupar(@"SELECT ISNULL(M.Apellido + ', ' + M.Nombre, 'Sin asignar') AS Etiqueta, COUNT(*) AS Cantidad
                             FROM Turno T
                             LEFT JOIN Medico M ON M.IdMedico = T.IdMedico
                             WHERE T.Estado = 'Confirmado' AND T.Fecha >= CAST(GETDATE() AS DATE)
                             GROUP BY ISNULL(M.Apellido + ', ' + M.Nombre, 'Sin asignar')
                             ORDER BY COUNT(*) DESC");
        }

        // ---------------------------------------------------------------
        // Método Agrupar (privado: solo se usa dentro de esta clase)
        // ---------------------------------------------------------------
        // Como los tres reportes devuelven lo mismo (Etiqueta y Cantidad),
        // en lugar de repetir tres veces el código de conexión lo escribimos
        // una sola vez acá. Cada reporte solo le pasa su consulta SQL.
        //
        // Pasos que sigue:
        //   1. Crea una lista vacía donde se van a guardar los resultados.
        //   2. Abre la conexión a la base de datos.
        //   3. Ejecuta la consulta.
        //   4. Recorre el resultado fila por fila y convierte cada fila en un
        //      objeto ResumenFila.
        //   5. Cierra la conexión y devuelve la lista.
        private static List<ResumenFila> Agrupar(string sql)
        {
            // Paso 1: lista vacía para ir guardando las filas.
            List<ResumenFila> lista = new List<ResumenFila>();

            // Paso 2: obtenemos la conexión. Conexion.Obtener() lee la cadena de
            // conexión del Web.config, así la dirección de la base no queda
            // escrita en el código.
            // El bloque "using" garantiza que la conexión se cierre al terminar,
            // incluso si ocurre un error en el medio. Así no quedan conexiones
            // abiertas ocupando recursos del servidor.
            using (SqlConnection cn = Conexion.Obtener())
            {
                // Paso 3: SqlCommand representa la consulta que vamos a ejecutar
                // sobre esa conexión.
                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();

                // ExecuteReader ejecuta la consulta y devuelve un SqlDataReader,
                // que permite leer el resultado de a una fila por vez.
                SqlDataReader reader = cmd.ExecuteReader();

                // Paso 4: reader.Read() avanza a la siguiente fila y devuelve
                // false cuando ya no quedan más. Por eso el while termina solo.
                while (reader.Read())
                {
                    // Creamos un objeto ResumenFila y le copiamos los dos valores
                    // de la fila actual, buscándolos por el nombre de la columna.
                    ResumenFila f = new ResumenFila();
                    f.Etiqueta = reader["Etiqueta"].ToString();
                    f.Cantidad = (int)reader["Cantidad"]; // COUNT(*) devuelve un número entero, por eso el (int)
                    lista.Add(f);
                }
            } // Paso 5: al salir del bloque "using" la conexión se cierra automáticamente.

            return lista;
        }
    }
}
