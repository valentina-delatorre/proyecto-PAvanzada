namespace BIZ.Modelo
{
    public class UsuarioListado
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public string NombreCompleto { get; set; }
        public string Dni { get; set; }
        public string Especialidad { get; set; }
    }
}