namespace Consultorio2026.Medicos
{
    // Panel generico: para medicos cuya especialidad no tiene pagina propia.
    public partial class PanelMedico : PanelMedicoBase
    {
        protected override string EspecialidadDelPanel { get { return null; } }
    }
}
