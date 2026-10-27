using System;
using System.Collections.Generic;
using System.Web;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Esto corre ANTES que el Page_Load de cualquier página (Init va antes que Load),
            // por eso sirve de "portero" para todas las pantallas que usan este master.
            //
            // El problema que evita: el login se recuerda de dos formas distintas.
            //   - La cookie de login vive en el NAVEGADOR y dura 30 minutos.
            //   - La sesión (Session["IdUsuario"], el rol, etc.) vive en la MEMORIA DEL SERVIDOR.
            // Si se reinicia la app (por ejemplo al recompilar) la memoria se borra, pero la cookie sigue.
            // Entonces la app te deja pasar sin saber quién sos, y al buscar Session["IdUsuario"] explota.
            // Si pasa eso, cerramos el login y lo mandamos a entrar de nuevo.
            if (Request.IsAuthenticated && Session["IdUsuario"] == null)
            {
                System.Web.Security.FormsAuthentication.SignOut();
                Response.Redirect("~/Seguridad/Login.aspx");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Rol"] != null)
            {
                string rol = Session["Rol"].ToString();
                string nombre = Session["NombreUsuario"] != null ? Session["NombreUsuario"].ToString() : "";

                LbUsuarioLogueado.Text = nombre + " · " + rol;

                // Los modulos a los que entra este usuario (su rol + lo que le cambio Seguridad).
                // El menu los usa para avisar "no tenés acceso" antes de ir a la pagina.
                List<string> modulos = AccesoDatos.ModulosHabilitados((int)Session["IdUsuario"]);
                var json = new System.Web.Script.Serialization.JavaScriptSerializer();
                LtRolScript.Text = "<script>var accesos = " + json.Serialize(modulos) + ";</script>";

                LiAccesos.Visible = modulos.Contains(AccesoDatos.ModuloAccesos);

                PhAsistente.Visible = modulos.Contains(AccesoDatos.ModuloAsistente);
                if (PhAsistente.Visible)
                    CargarAsistente(rol);
            }
        }

        // Las preguntas de ejemplo cambian segun quien este logueado
        private void CargarAsistente(string rol)
        {
            string[] sugerencias;
            if (rol == Roles.Paciente)
            {
                LtAsistenteSub.Text = "Te digo qué días hay lugar y te saco el turno";
                sugerencias = new[] { "¿Qué días hay turno en Cardiologia?", "Quiero un turno de Pediatria lo antes posible", "¿Cuáles son mis próximos turnos?" };
            }
            else if (rol == Roles.Medico)
            {
                LtAsistenteSub.Text = "Tu día de hoy, tus pacientes y tus horarios";
                sugerencias = new[] { "¿Cuántos pacientes tengo hoy y a qué hora?", "¿Qué horarios me quedan libres hoy?", "¿Cómo viene mi semana?" };
            }
            else
            {
                LtAsistenteSub.Text = "Preguntale por los turnos del consultorio";
                sugerencias = new[] { "¿Qué día de la semana se piden más turnos?", "¿Qué especialidad tiene más demanda?", "¿Cuántos turnos quedan pendientes de confirmar?" };
            }

            string html = "";
            foreach (string s in sugerencias)
                html += "<button type=\"button\" class=\"asistente-chip\">" + HttpUtility.HtmlEncode(s) + "</button>";
            LtSugerencias.Text = html;
        }

        // La direccion de un CSS o JS con su "version" (la fecha en que se modifico el archivo).
        // Asi el navegador no se queda con una copia vieja guardada despues de un cambio.
        protected string Recurso(string ruta)
        {
            string archivo = Server.MapPath(ruta);
            long version = System.IO.File.Exists(archivo) ? System.IO.File.GetLastWriteTimeUtc(archivo).Ticks : 0;
            return ResolveUrl(ruta) + "?v=" + version;
        }

        protected void BtCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            System.Web.Security.FormsAuthentication.SignOut();
            Response.Redirect("~/Seguridad/Login.aspx");
        }
    }
}