using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BIZ.Modelo;
using BIZ.Datos;

namespace Consultorio2026.Reportes
{
    // =====================================================================
    // PÁGINA ReporteDiario - Código de la pantalla del módulo Reportes
    // =====================================================================
    // Este archivo es el "code-behind": el código C# que acompaña a la página
    // ReporteDiario.aspx. La página .aspx define cómo se ve la pantalla (las
    // tablas) y este archivo se encarga de llenarlas con datos.
    //
    // Seguridad: a esta página solo entra quien tenga habilitado el módulo
    // "Ver Reportes" (por su rol o porque Seguridad se lo habilitó a mano en
    // Seguridad/Accesos.aspx). Eso no se controla acá sino en Global.asax,
    // que antes de abrir cualquier página consulta la tabla URL y la vista
    // VW_ACCESOS de la base.
    // =====================================================================
    public partial class ReporteDiario : System.Web.UI.Page
    {
        // Page_Load es el evento que ASP.NET ejecuta automáticamente cada vez
        // que se carga la página. Acá buscamos los datos y llenamos las tablas.
        protected void Page_Load(object sender, EventArgs e)
        {
            // IsPostBack indica si la página se está recargando porque el usuario
            // hizo una acción dentro de ella (por ejemplo, apretar un botón).
            // Con "!IsPostBack" ejecutamos este bloque solo la primera vez que se
            // abre la página, para no volver a consultar la base sin necesidad.
            if (!IsPostBack)
            {
                // Mostramos la fecha de hoy arriba de todo, escrita en castellano:
                // "martes 27 de octubre de 2026". CultureInfo("es-AR") es lo que
                // hace que los nombres de los días y meses salgan en español.
                LtFechaHoy.Text = DateTime.Today.ToString("dddd d 'de' MMMM 'de' yyyy", new CultureInfo("es-AR"));

                // Las cuatro tablas se llenan de la misma manera, en dos pasos:
                //   - DataSource: le indicamos a la tabla de dónde salen los datos
                //     (la lista que devuelve el método de la capa de datos).
                //   - DataBind(): le pedimos que dibuje las filas con esos datos.

                // Tabla 1 - Turnos de hoy.
                // Reutilizamos el método ListarDelDia() que ya existía en la clase
                // TurnoDatos (la del módulo Turnos), en vez de escribir otra consulta igual.
                List<Turno> deHoy = TurnoDatos.ListarDelDia();
                GvHoy.DataSource = deHoy;
                GvHoy.DataBind();

                // Tabla 2 - Cantidad de turnos en cada estado.
                List<ResumenFila> estados = ReporteDatos.TotalesPorEstado();
                GvEstados.DataSource = estados;
                GvEstados.DataBind();

                // Los tres números grandes de la cabecera salen de las mismas listas,
                // así no hace falta volver a consultar la base:
                //   - Hoy: cuántas filas tiene la tabla de hoy.
                //   - Total: la suma de todos los estados (Sum suma la columna Cantidad).
                //   - Confirmados: la fila del estado "Confirmado", si existe.
                LtHoy.Text = deHoy.Count.ToString();
                LtTotal.Text = estados.Sum(f => f.Cantidad).ToString();
                LtConfirmados.Text = estados.Where(f => f.Etiqueta == "Confirmado").Sum(f => f.Cantidad).ToString();

                // Tabla 3 - Cantidad de turnos de cada especialidad.
                GvEspecialidades.DataSource = ReporteDatos.TurnosPorEspecialidad();
                GvEspecialidades.DataBind();

                // Tabla 4 - Turnos confirmados a futuro de cada médico.
                GvProximos.DataSource = ReporteDatos.ProximosPorMedico();
                GvProximos.DataBind();
            }
        }
    }
}
