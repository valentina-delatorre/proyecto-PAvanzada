using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace BIZ
{
    // Habla con Ollama, el programa que corre el modelo de IA en esta misma maquina.
    // La direccion y el modelo salen del Web.config (appSettings OllamaUrl y OllamaModelo).
    // Se usa la API /api/chat: le mandamos la conversacion y nos devuelve la respuesta entera.
    public class Ollama
    {
        private static string Url
        {
            get { return (ConfigurationManager.AppSettings["OllamaUrl"] ?? "http://localhost:11434").TrimEnd('/'); }
        }

        public static string Modelo
        {
            get { return ConfigurationManager.AppSettings["OllamaModelo"] ?? "qwen2.5:7b"; }
        }

        // mensajes: lista de { role = "system" | "user" | "assistant", content = "..." }
        // formato:  null para texto libre, o un esquema JSON para obligarlo a contestar con esa forma
        public static string Chat(List<Dictionary<string, string>> mensajes, object formato)
        {
            var json = new JavaScriptSerializer();

            var pedido = new Dictionary<string, object>
            {
                { "model", Modelo },
                { "messages", mensajes },
                { "stream", false },
                { "keep_alive", "30m" },   // que quede cargado en memoria: la primera respuesta es la lenta
                { "options", new Dictionary<string, object> { { "temperature", 0.2 }, { "num_ctx", 8192 } } }
            };
            if (formato != null)
                pedido["format"] = formato;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url + "/api/chat");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Timeout = 180000;   // la primera vez tiene que cargar el modelo en la placa de video

            byte[] cuerpo = Encoding.UTF8.GetBytes(json.Serialize(pedido));
            using (Stream s = req.GetRequestStream())
                s.Write(cuerpo, 0, cuerpo.Length);

            using (WebResponse resp = req.GetResponse())
            using (StreamReader lector = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
            {
                var respuesta = json.Deserialize<Dictionary<string, object>>(lector.ReadToEnd());
                var mensaje = (Dictionary<string, object>)respuesta["message"];
                return mensaje["content"].ToString().Trim();
            }
        }

        // Para el cartel de "no disponible": ¿esta prendido Ollama?
        public static bool Disponible()
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url + "/api/version");
                req.Timeout = 2000;
                using (req.GetResponse()) return true;
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
