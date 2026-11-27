namespace Aplicacion.Modelos
{
    // Resumen del dinero del refugio: saldo inicial + todo lo donado
    public class DonacionResumen
    {
        public decimal SaldoInicial { get; set; }
        public decimal TotalDonado { get; set; }
        public decimal FondoTotal { get; set; }
        public int CantidadDonaciones { get; set; }
        public DateTime? UltimaDonacion { get; set; }
    }
}
