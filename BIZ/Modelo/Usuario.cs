using System;

namespace BIZ.Modelo
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasenia { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
    }
}