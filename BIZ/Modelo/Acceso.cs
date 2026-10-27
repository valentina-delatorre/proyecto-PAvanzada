namespace BIZ.Modelo
{
    // Un modulo del sistema y si un usuario puntual puede entrar o no.
    public class Acceso
    {
        public int IdUsuario { get; set; }
        public int IdPermiso { get; set; }
        public string Modulo { get; set; }
        public bool Habilitado { get; set; }

        // Lo que le daria su rol. Si Habilitado es distinto, es un cambio hecho a mano.
        public bool PorRol { get; set; }
    }
}
