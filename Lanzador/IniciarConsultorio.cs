// IniciarConsultorio.exe
// Deja todo andando para presentar el sistema con un doble clic:
//   1. SQL Server: lo prende si esta apagado y aplica los scripts 07 y 08 (se pueden repetir).
//   2. Ollama (el asistente con IA): lo prende, baja el modelo si falta y lo precalienta.
//   3. Compila la solucion si hace falta (si no esta el .dll o el codigo es mas nuevo).
//   4. Levanta el sitio en IIS Express (http://localhost:59042).
//   5. Abre el navegador en la portada y en el login.
//
// Va en la carpeta raiz del repo (al lado de Consultorio2026.slnx).
// Para recompilarlo: Lanzador\compilar.bat

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

class IniciarConsultorio
{
    const string Sitio = "http://localhost:59042";
    const int Puerto = 59042;

    static string raiz;
    static bool huboErrores;

    static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "Consultorio2026 - Inicio";
        Titulo("CONSULTORIO 2026 · poniendo todo en marcha");

        raiz = BuscarRaiz();
        if (raiz == null)
        {
            Error("No encuentro el proyecto. Poné este .exe en la carpeta proyecto-PAvanzada (al lado de Consultorio2026.slnx).");
            return Terminar(1);
        }
        Ok("Proyecto: " + raiz);

        string modelo = LeerModelo();
        Paso(1, "Base de datos");
        BaseDeDatos();

        Paso(2, "Asistente con IA (Ollama, modelo " + modelo + ")");
        Thread ollama = new Thread(() => Ollama(modelo));   // corre en paralelo mientras se compila
        ollama.Start();

        Paso(3, "Compilación");
        if (!Compilar())
            return Terminar(1);

        Paso(4, "Sitio web (IIS Express)");
        if (!LevantarSitio())
            return Terminar(1);

        ollama.Join();

        Paso(5, "Abriendo el navegador");
        Abrir(Sitio + "/Default.aspx");
        Abrir(Sitio + "/Seguridad/Login.aspx");

        Titulo(huboErrores ? "LISTO, CON AVISOS (leé arriba)" : "TODO LISTO");
        Console.WriteLine("  Sitio:  " + Sitio);
        Console.WriteLine("  Clave de los usuarios del grupo: Usal2026");
        Console.WriteLine("    Seguridad (Valentina)  valentina.d@usal.edu.ar");
        Console.WriteLine("    Usuarios  (Micaela)    micaelamorena.candore@usal.edu.ar");
        Console.WriteLine("    Turnos    (Martino)    martino.dominguez@usal.edu.ar");
        Console.WriteLine("    Reportes  (Agustín)    a.villalpando@usal.edu.ar");
        Console.WriteLine("    Médico                 j.rios@consultorio.com");
        Console.WriteLine("    Paciente               carla.benitez@mail.com  (clave paciente123)");
        Console.WriteLine();
        Console.WriteLine("  El sitio y Ollama siguen corriendo aunque cierres esta ventana.");
        return Terminar(0);
    }

    /* ----- 1. Base de datos ------------------------------------------------ */

    static void BaseDeDatos()
    {
        string cadena = LeerCadenaConexion();
        if (cadena == null) { Aviso("No pude leer la conexión ConsultorioDB del Web.config."); return; }

        string servidor = Valor(cadena, "Data Source") ?? Valor(cadena, "Server") ?? @".\SQLEXPRESS";
        Ok("Servidor: " + servidor);

        // Si es una instancia local (".\NOMBRE" o "(local)\NOMBRE"), prendemos su servicio
        Match local = Regex.Match(servidor, @"^(\.|\(local\)|localhost|" + Regex.Escape(Environment.MachineName) + @")\\(.+)$", RegexOptions.IgnoreCase);
        if (local.Success)
        {
            string servicio = "MSSQL$" + local.Groups[2].Value;
            try
            {
                using (ServiceController sc = new ServiceController(servicio))
                {
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        Info("Prendiendo el servicio " + servicio + "...");
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(40));
                    }
                    Ok("SQL Server prendido.");
                }
            }
            catch (Exception ex)
            {
                Aviso("No pude prender " + servicio + " (" + ex.Message + "). Si falla, abrí 'Servicios' y prendelo a mano.");
            }
        }

        string sqlcmd = Buscar("sqlcmd.exe");
        if (sqlcmd == null) { Aviso("No encontré sqlcmd: no apliqué los scripts 07 y 08 (corrélos desde SSMS)."); return; }

        string usuario = Valor(cadena, "User ID");
        string clave = Valor(cadena, "Password");
        string base_ = Valor(cadena, "Initial Catalog") ?? "ConsultorioI";
        string login = usuario == null ? "-E" : "-U \"" + usuario + "\" -P \"" + clave + "\"";

        foreach (string script in new[] { "07_procedimientos.sql", "08_accesos.sql" })
        {
            string ruta = Path.Combine(raiz, "BaseDatos", script);
            string salida;
            // -C confia en el certificado del SQL local (el driver ODBC 18 lo rechaza si no)
            int codigo = Ejecutar(sqlcmd, "-S \"" + servidor + "\" -d " + base_ + " " + login + " -C -I -b -i \"" + ruta + "\"", out salida, 120);
            if (codigo == 0) Ok(script + " aplicado.");
            else Aviso(script + " falló: " + Ultimas(salida, 3));
        }
    }

    /* ----- 2. Ollama ------------------------------------------------------- */

    static void Ollama(string modelo)
    {
        try
        {
            if (!Responde("http://localhost:11434/api/version"))
            {
                string exe = Buscar("ollama.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Ollama\ollama.exe"));
                if (exe == null) { Aviso("Ollama no está instalado: el sitio anda igual, pero el asistente no. Instalalo con: winget install Ollama.Ollama"); return; }
                Info("Prendiendo Ollama...");
                Process.Start(new ProcessStartInfo(exe, "serve") { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden });
                if (!Esperar("http://localhost:11434/api/version", 30)) { Aviso("Ollama no arrancó."); return; }
            }
            Ok("Ollama prendido.");

            string tags = new WebClient().DownloadString("http://localhost:11434/api/tags");
            if (!tags.Contains("\"" + modelo + "\""))
            {
                Info("Bajando el modelo " + modelo + " (son unos GB, la primera vez tarda)...");
                string exe = Buscar("ollama.exe", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Ollama\ollama.exe"));
                string salida;
                if (Ejecutar(exe, "pull " + modelo, out salida, 3600) != 0) { Aviso("No pude bajar el modelo: " + Ultimas(salida, 2)); return; }
            }

            // Una pregunta cortita para que cargue el modelo en memoria: así la primera de verdad sale rápido
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                wc.UploadString("http://localhost:11434/api/generate",
                    "{\"model\":\"" + modelo + "\",\"prompt\":\"hola\",\"stream\":false,\"keep_alive\":\"30m\",\"options\":{\"num_predict\":1}}");
            }
            Ok("Asistente listo (modelo cargado).");
        }
        catch (Exception ex)
        {
            Aviso("Asistente: " + ex.Message);
        }
    }

    /* ----- 3. Compilacion -------------------------------------------------- */

    static bool Compilar()
    {
        string dll = Path.Combine(raiz, @"Consultorio2026\bin\Consultorio2026.dll");
        DateTime ultimoCambio = Directory.GetFiles(raiz, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(raiz, "*.csproj", SearchOption.AllDirectories))
            .Where(f => !f.Contains(@"\bin\") && !f.Contains(@"\obj\") && !f.Contains(@"\Lanzador\"))
            .Select(File.GetLastWriteTime).Max();

        if (File.Exists(dll) && File.GetLastWriteTime(dll) >= ultimoCambio)
        {
            Ok("Ya está compilado y al día.");
            return true;
        }

        string msbuild = BuscarMsbuild();
        if (msbuild == null)
        {
            Error("No encontré MSBuild. Abrí la solución en Visual Studio y compilala una vez (Ctrl+Shift+B).");
            return false;
        }

        Info("Compilando (puede tardar un minuto)...");
        string proyecto = Path.Combine(raiz, @"Consultorio2026\Consultorio2026.csproj");
        string salida;
        // /t:Restore baja los paquetes NuGet (la carpeta packages no está en git)
        Ejecutar(msbuild, "\"" + proyecto + "\" /t:Restore /p:RestorePackagesConfig=true /nologo /v:q", out salida, 300);
        if (Ejecutar(msbuild, "\"" + proyecto + "\" /nologo /v:m", out salida, 600) != 0)
        {
            Error("La compilación falló:\n" + Ultimas(salida, 15));
            return false;
        }
        Ok("Compilado.");
        return true;
    }

    static string BuscarMsbuild()
    {
        string vswhere = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft Visual Studio\Installer\vswhere.exe");
        if (File.Exists(vswhere))
        {
            string salida;
            Ejecutar(vswhere, "-latest -requires Microsoft.Component.MSBuild -find MSBuild\\**\\Bin\\MSBuild.exe", out salida, 30);
            string ruta = salida.Split('\n').Select(l => l.Trim()).FirstOrDefault(File.Exists);
            if (ruta != null) return ruta;
        }
        return Buscar("msbuild.exe");
    }

    /* ----- 4. IIS Express -------------------------------------------------- */

    static bool LevantarSitio()
    {
        if (Responde(Sitio + "/Seguridad/Login.aspx"))
        {
            Ok("El sitio ya estaba corriendo.");
            return true;
        }

        string iis = new[] {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"IIS Express\iisexpress.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"IIS Express\iisexpress.exe")
        }.FirstOrDefault(File.Exists);
        if (iis == null)
        {
            Error("No encontré IIS Express. Se instala con Visual Studio (carga de trabajo 'ASP.NET y desarrollo web').");
            return false;
        }

        Info("Levantando el sitio en el puerto " + Puerto + "...");
        Process.Start(new ProcessStartInfo(iis, "/path:\"" + Path.Combine(raiz, "Consultorio2026") + "\" /port:" + Puerto)
        {
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Minimized   // queda minimizado en la barra de tareas
        });

        // La primera carga compila las paginas: puede tardar
        if (!Esperar(Sitio + "/Seguridad/Login.aspx", 90))
        {
            Error("El sitio no respondió. Mirá la ventana de IIS Express minimizada.");
            return false;
        }
        Ok("Sitio arriba en " + Sitio);
        return true;
    }

    /* ----- Ayudas ---------------------------------------------------------- */

    static string BuscarRaiz()
    {
        foreach (string inicio in new[] { AppDomain.CurrentDomain.BaseDirectory, Environment.CurrentDirectory })
        {
            DirectoryInfo d = new DirectoryInfo(inicio);
            while (d != null)
            {
                if (File.Exists(Path.Combine(d.FullName, @"Consultorio2026\Consultorio2026.csproj"))) return d.FullName;
                d = d.Parent;
            }
        }
        string porDefecto = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "proyecto-PAvanzada");
        return File.Exists(Path.Combine(porDefecto, @"Consultorio2026\Consultorio2026.csproj")) ? porDefecto : null;
    }

    static string WebConfig() { return File.ReadAllText(Path.Combine(raiz, @"Consultorio2026\Web.config")); }

    static string LeerCadenaConexion()
    {
        Match m = Regex.Match(WebConfig(), "name=\"ConsultorioDB\"\\s+connectionString=\"([^\"]+)\"");
        return m.Success ? m.Groups[1].Value : null;
    }

    static string LeerModelo()
    {
        Match m = Regex.Match(WebConfig(), "key=\"OllamaModelo\"\\s+value=\"([^\"]+)\"");
        return m.Success ? m.Groups[1].Value : "qwen2.5:7b";
    }

    static string Valor(string cadena, string clave)
    {
        Match m = Regex.Match(cadena, "(^|;)\\s*" + Regex.Escape(clave) + "\\s*=\\s*([^;]+)", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[2].Value.Trim() : null;
    }

    static string Buscar(string exe, params string[] extras)
    {
        foreach (string e in extras) if (File.Exists(e)) return e;
        foreach (string dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
        {
            try { string r = Path.Combine(dir.Trim(), exe); if (File.Exists(r)) return r; } catch { }
        }
        return null;
    }

    static int Ejecutar(string exe, string args, out string salida, int segundos)
    {
        ProcessStartInfo psi = new ProcessStartInfo(exe, args)
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8
        };
        using (Process p = Process.Start(psi))
        {
            StringBuilder sb = new StringBuilder();
            p.OutputDataReceived += (s, e) => { if (e.Data != null) lock (sb) sb.AppendLine(e.Data); };
            p.ErrorDataReceived += (s, e) => { if (e.Data != null) lock (sb) sb.AppendLine(e.Data); };
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            if (!p.WaitForExit(segundos * 1000)) { try { p.Kill(); } catch { } salida = sb + "\n(tardó demasiado)"; return -1; }
            p.WaitForExit();
            salida = sb.ToString();
            return p.ExitCode;
        }
    }

    static bool Responde(string url)
    {
        try
        {
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);
            r.Timeout = 3000;
            using (r.GetResponse()) return true;
        }
        catch { return false; }
    }

    static bool Esperar(string url, int segundos)
    {
        for (int i = 0; i < segundos; i++)
        {
            if (Responde(url)) return true;
            Thread.Sleep(1000);
        }
        return false;
    }

    static void Abrir(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); Ok(url); }
        catch (Exception ex) { Aviso("No pude abrir " + url + ": " + ex.Message); }
    }

    static string Ultimas(string texto, int n)
    {
        string[] l = texto.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("\n", l.Skip(Math.Max(0, l.Length - n)).Select(x => "      " + x.Trim()));
    }

    static readonly object consola = new object();
    static void Escribir(ConsoleColor color, string marca, string texto)
    {
        lock (consola)
        {
            Console.ForegroundColor = color; Console.Write("  " + marca + " ");
            Console.ResetColor(); Console.WriteLine(texto);
        }
    }
    static void Titulo(string t) { lock (consola) { Console.WriteLine(); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine("  " + t); Console.ResetColor(); Console.WriteLine(); } }
    static void Paso(int n, string t) { lock (consola) { Console.WriteLine(); Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(" " + n + ". " + t); Console.ResetColor(); } }
    static void Ok(string t) { Escribir(ConsoleColor.Green, "✓", t); }
    static void Info(string t) { Escribir(ConsoleColor.Gray, "·", t); }
    static void Aviso(string t) { huboErrores = true; Escribir(ConsoleColor.DarkYellow, "!", t); }
    static void Error(string t) { huboErrores = true; Escribir(ConsoleColor.Red, "✗", t); }

    static int Terminar(int codigo)
    {
        Console.WriteLine();
        Console.Write("  Apretá una tecla para cerrar esta ventana...");
        try { Console.ReadKey(true); } catch { }
        return codigo;
    }
}
