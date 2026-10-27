using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using BIZ;
using BIZ.Datos;
using BIZ.Modelo;

namespace Consultorio2026.Asistente
{
    // Lo llama Content/Asistente.js con fetch. Recibe y devuelve JSON.
    //   accion "preguntar": { pregunta, historial[] }      -> { texto, propuesta }
    //   accion "reservar":  { especialidad, fecha, hora, medico, motivo } -> { texto }
    // IRequiresSessionState es para poder leer Session["IdUsuario"] y Session["Rol"].
    // Si el usuario no tiene el modulo "Asistente IA", Global.asax lo frena antes de llegar aca.
    public class Consultar : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            var json = new JavaScriptSerializer();
            context.Response.ContentType = "application/json";

            if (context.Session["IdUsuario"] == null || context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 401;
                return;
            }

            int idUsuario = (int)context.Session["IdUsuario"];
            string rol = context.Session["Rol"].ToString();

            string cuerpo;
            using (StreamReader lector = new StreamReader(context.Request.InputStream))
                cuerpo = lector.ReadToEnd();
            var pedido = json.Deserialize<Dictionary<string, object>>(cuerpo);

            object resultado;
            try
            {
                if (Texto(pedido, "accion") == "reservar")
                    resultado = Reservar(idUsuario, rol, pedido);
                else
                    resultado = Preguntar(idUsuario, rol, pedido);
            }
            catch (WebException)
            {
                resultado = new
                {
                    texto = Ollama.Disponible()
                        ? "El asistente tardó demasiado en contestar. Probá de nuevo en unos segundos."
                        : "El asistente no está disponible: Ollama no está corriendo en el servidor."
                };
            }

            context.Response.Write(json.Serialize(resultado));
        }

        private static object Preguntar(int idUsuario, string rol, Dictionary<string, object> pedido)
        {
            string pregunta = Texto(pedido, "pregunta");
            if (pregunta.Length == 0)
                return new { texto = "Escribí tu pregunta." };
            if (pregunta.Length > 400) pregunta = pregunta.Substring(0, 400);

            // Lo ultimo que se hablo, para que entienda "¿y el jueves?" o "dale, ese"
            var historial = new List<Dictionary<string, string>>();
            object[] previos = pedido.ContainsKey("historial") ? pedido["historial"] as object[] : null;
            if (previos != null)
                for (int i = Math.Max(0, previos.Length - 6); i < previos.Length; i++)
                {
                    var m = previos[i] as Dictionary<string, object>;
                    string quien = m == null ? "" : Texto(m, "role");
                    if (quien == "user" || quien == "assistant")
                        historial.Add(AsistenteIA.Mensaje(quien, Recortar(Texto(m, "content"), 1500)));
                }

            AsistenteIA.Respuesta r = AsistenteIA.Preguntar(idUsuario, rol, pregunta, historial);
            return new { texto = r.Texto, propuesta = r.Propuesta };
        }

        // El paciente apreto "Confirmar turno" en la tarjeta que le propuso el asistente
        private static object Reservar(int idUsuario, string rol, Dictionary<string, object> pedido)
        {
            if (rol != Roles.Paciente)
                return new { texto = "Solo los pacientes pueden sacar turnos desde el asistente." };

            // Se vuelve a revisar contra la base: puede que alguien lo haya tomado mientras tanto.
            // El medico se vuelve a buscar por nombre: no confiamos en un id que manda el navegador.
            AsistenteIA.Propuesta p = AsistenteIA.ValidarPropuesta(Texto(pedido, "especialidad"), Texto(pedido, "fecha"),
                                                                   Texto(pedido, "hora"), Texto(pedido, "medico"));
            if (p == null)
                return new { texto = "Ese horario ya no está disponible. Pedime otro." };

            string motivo = Texto(pedido, "motivo");
            if (motivo != "Primera consulta" && motivo != "Control" && motivo != "Estudio")
                motivo = "Primera consulta";

            Turno t = new Turno();
            t.IdUsuarioPaciente = idUsuario;
            t.IdMedico = p.IdMedico > 0 ? (int?)p.IdMedico : null;
            t.Especialidad = p.Especialidad;
            t.TipoConsulta = motivo;
            t.Fecha = DateTime.ParseExact(p.Fecha, "yyyy-MM-dd", null);
            t.Hora = p.Hora;
            t.Observaciones = "Pedido desde el asistente";
            TurnoDatos.Solicitar(t);

            return new
            {
                texto = "✓ Listo, pedí tu turno de " + p.Especialidad + " para el " + p.FechaTexto + " a las " + p.Hora
                      + " hs. Te va a aparecer como Confirmado en \"Mis turnos\" cuando el consultorio lo apruebe.",
                reservado = true
            };
        }

        private static string Texto(Dictionary<string, object> d, string clave)
        {
            return d != null && d.ContainsKey(clave) && d[clave] != null ? d[clave].ToString().Trim() : "";
        }

        private static string Recortar(string s, int max)
        {
            return s.Length > max ? s.Substring(0, max) : s;
        }
    }
}
