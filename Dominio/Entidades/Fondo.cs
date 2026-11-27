namespace Dominio.Entidades
{
    // Dinero con el que ya cuenta el refugio. Solo hay una fila; a este saldo se le suman las donaciones.
    public class Fondo
    {
        public int FondoId { get; set; }

        public decimal SaldoInicial { get; set; }

        public DateTime FechaActualizacion { get; set; }
    }
}
