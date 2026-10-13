using System;

namespace BIZ.Modelo
{
    // Un dia del almanaque de un profesional (lo devuelve SP_AGENDA_MES)
    public class DiaAgenda
    {
        public DateTime Fecha { get; set; }
        public int Dia { get; set; }
        public int Turnos { get; set; }
        public string Horarios { get; set; }
    }
}
