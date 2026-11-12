namespace KemilEditorial.API.Auth
{
    // Deben coincidir EXACTAMENTE con rol.nombre_rol en la base de datos
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Asesor = "Asesor";
        public const string Cliente = "Cliente";
        public const string Mayorista = "Mayorista";
        public const string Institucional = "Institucional";

        // Combinaciones para [Authorize(Roles = ...)]
        public const string Personal = Administrador + "," + Asesor;
        public const string Compradores = Cliente + "," + Mayorista + "," + Institucional;
        public const string SolicitantesCotizacion = Mayorista + "," + Institucional;
    }
}