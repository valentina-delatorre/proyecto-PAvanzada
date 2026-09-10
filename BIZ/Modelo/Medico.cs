namespace BIZ.Modelo
{
    public class Medico
    {
        public int IdMedico { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Especialidad { get; set; }
        public bool Activo { get; set; }
        public string UsuarioVinculado { get; set; }

        public string NombreCompleto
        {
            get { return Apellido + ", " + Nombre; }
        }
    }
}