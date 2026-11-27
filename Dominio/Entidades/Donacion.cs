namespace Dominio.Entidades
{
    public class Donacion
    {
        public int DonacionId { get; set; }

        public string NombreDonante { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public string Moneda { get; set; } = "MXN";

        public DateTime Fecha { get; set; }

        public string? IdTransaccion { get; set; }

        public string Estado { get; set; } = "Confirmada";

        // Cuenta que donó (tabla Usuarios). Es opcional: se puede donar sin sesión. Llave foránea; la pone el servidor.
        public int? UsuarioId { get; set; }

        // Fondo (tabla Fondos) al que se suma esta donación. Llave foránea; la pone el servidor.
        public int? FondoId { get; set; }
    }
}
