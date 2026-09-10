using System.Collections.Generic;

namespace Consultorio2026
{
    public class ServicioInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Especialidad { get; set; }
        public string Resumen { get; set; }
        public string Detalle { get; set; }
        public string ImagenCard { get; set; }
        public string[] ImagenesDetalle { get; set; }
    }

    public static class ServiciosCatalogo
    {
        public static List<ServicioInfo> Todos()
        {
            return new List<ServicioInfo>
            {
                new ServicioInfo { Id = 1, Nombre = "Cardiología", Especialidad = "Cardiología",
                    Resumen = "Controles, electrocardiogramas y seguimiento de pacientes con afecciones cardíacas.",
                    Detalle = "Realizamos controles cardiológicos completos: electrocardiogramas, ergometrías y seguimiento de presión arterial. Trabajamos en la prevención y el tratamiento de afecciones cardíacas con un plan personalizado para cada paciente.",
                    ImagenCard = "servicio1.png",
                    ImagenesDetalle = new[] { "detalleservicio1.png", "detalleservicio2.png", "detalleservicio3.png" } },

                new ServicioInfo { Id = 2, Nombre = "Pediatría", Especialidad = "Pediatría",
                    Resumen = "Atención integral de niños y adolescentes, controles de crecimiento y vacunación.",
                    Detalle = "Acompañamos el crecimiento de tus hijos con controles periódicos, calendario de vacunación completo y atención de las consultas del día a día, en un espacio pensado para que los más chicos se sientan cómodos.",
                    ImagenCard = "servicio2.png",
                    ImagenesDetalle = new[] { "detalleservicio4.png", "detalleservicio5.png", "detalleservicio6.png" } },

                new ServicioInfo { Id = 3, Nombre = "Traumatología", Especialidad = "Traumatología",
                    Resumen = "Diagnóstico y tratamiento de lesiones, rehabilitación y seguimiento kinesiológico.",
                    Detalle = "Atendemos lesiones deportivas, fracturas, esguinces y dolores articulares. Diagnóstico por imágenes, inmovilizaciones y plan de rehabilitación coordinado con nuestro equipo de kinesiología.",
                    ImagenCard = "servicio3.png",
                    ImagenesDetalle = new[] { "detalleservicio7.png", "detalleservicio8.png", "detalleservicio9.png" } },

                new ServicioInfo { Id = 4, Nombre = "Clínica Médica", Especialidad = "Clínica Médica",
                    Resumen = "Consultas generales, chequeos anuales y seguimiento de enfermedades crónicas.",
                    Detalle = "La puerta de entrada al sistema de salud: consultas generales, chequeos preventivos anuales, control de enfermedades crónicas como diabetes e hipertensión, y derivación al especialista correcto cuando hace falta.",
                    ImagenCard = "servicio4.png",
                    ImagenesDetalle = new[] { "detalleservicio10.png", "detalleservicio11.png", "detalleservicio12.png" } },

                new ServicioInfo { Id = 5, Nombre = "Dermatología", Especialidad = "Dermatología",
                    Resumen = "Control de lunares, tratamiento de afecciones de la piel y dermatología estética.",
                    Detalle = "Controles anuales de lunares con dermatoscopía, tratamiento de acné, dermatitis y otras afecciones de la piel, y procedimientos de dermatología estética realizados por profesionales matriculados.",
                    ImagenCard = "servicio5.png",
                    ImagenesDetalle = new[] { "detalleservicio13.png", "detalleservicio14.png", "detalleservicio15.png" } },

                new ServicioInfo { Id = 6, Nombre = "Kinesiología", Especialidad = "Kinesiología",
                    Resumen = "Rehabilitación de lesiones, fisioterapia y planes de recuperación personalizados.",
                    Detalle = "Sesiones de kinesiología y fisioterapia para la recuperación de lesiones, postoperatorios y dolores posturales. Cada plan se arma en conjunto con el médico derivante y se ajusta según tu evolución.",
                    ImagenCard = "servicio6.png",
                    ImagenesDetalle = new[] { "detalleservicio16.png", "detalleservicio17.png", "detalleservicio18.png" } }
            };
        }

        public static ServicioInfo Obtener(int id)
        {
            return Todos().Find(s => s.Id == id);
        }
    }
}