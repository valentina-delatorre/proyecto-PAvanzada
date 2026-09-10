using System;

namespace BIZ.Modelo
{
    public class Turno
    {
        public int IdTurno { get; set; }
        public int IdUsuarioPaciente { get; set; }
        public int? IdMedico { get; set; }
        public string Especialidad { get; set; }
        public string TipoConsulta { get; set; }
        public DateTime Fecha { get; set; }
        public string Hora { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }

        public string NombrePaciente { get; set; }
        public string NombreMedico { get; set; }

        // Calculado al mostrar: nunca queda desactualizado
        public string EstadoVisual
        {
            get
            {
                if (Estado == "Confirmado")
                    return Fecha.Date < DateTime.Today ? "Atendido" : "Próximo";
                return Estado; // Solicitado / Rechazado
            }
        }
    }
}