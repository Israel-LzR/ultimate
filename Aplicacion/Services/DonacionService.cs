using Aplicacion.Interfaces;
using Aplicacion.Modelos;
using Dominio.Entidades;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class DonacionService : IDonacionService
    {
        private readonly IDonacionRepository _donacionRepository;
        private readonly IFondoRepository _fondoRepository;

        public DonacionService(IDonacionRepository donacionRepository, IFondoRepository fondoRepository)
        {
            _donacionRepository = donacionRepository;
            _fondoRepository = fondoRepository;
        }

        public async Task<List<Donacion>> ObtenerTodosAsync()
        {
            return await _donacionRepository.ObtenerTodosAsync();
        }

        public async Task<Donacion?> ObtenerPorIdAsync(int id)
        {
            return await _donacionRepository.ObtenerPorIdAsync(id);
        }

        public async Task AgregarAsync(Donacion donacion)
        {
            donacion.NombreDonante = (donacion.NombreDonante ?? "").Trim();
            if (donacion.NombreDonante.Length < 2 || donacion.NombreDonante.Length > 80)
                throw new ArgumentException("El nombre del donante debe tener entre 2 y 80 caracteres.");
            if (donacion.Monto <= 0 || donacion.Monto > 100000)
                throw new ArgumentException("El monto debe ser mayor a 0 y máximo 100,000.");
            donacion.Moneda = "MXN";

            // Reglas de negocio: el servidor decide fecha, estado e id de transacción
            donacion.Fecha = DateTime.Now;
            donacion.Estado = "Confirmada";
            donacion.IdTransaccion = "DON-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

            // Relación con la tabla Fondos: cada donación queda ligada al fondo al que se le suma
            var fondo = await _fondoRepository.ObtenerAsync();
            donacion.FondoId = fondo?.FondoId;

            await _donacionRepository.AgregarAsync(donacion);
        }

        // Fondo del refugio = saldo inicial (tabla Fondos) + suma de todas las donaciones confirmadas.
        // Cada donación nueva queda guardada en la base, así que el total siempre sube solo.
        // Si la tabla Fondos aún no tiene fila, se usa el saldo de respaldo (appsettings.json).
        public async Task<DonacionResumen> ObtenerResumenAsync(decimal saldoInicialRespaldo)
        {
            var fondo = await _fondoRepository.ObtenerAsync();
            var saldoInicial = fondo?.SaldoInicial ?? saldoInicialRespaldo;

            var donaciones = await _donacionRepository.ObtenerTodosAsync();
            var confirmadas = donaciones.Where(d => d.Estado == "Confirmada").ToList();
            var total = confirmadas.Sum(d => d.Monto);

            return new DonacionResumen
            {
                SaldoInicial = saldoInicial,
                TotalDonado = total,
                FondoTotal = saldoInicial + total,
                CantidadDonaciones = confirmadas.Count,
                UltimaDonacion = confirmadas.Count > 0 ? confirmadas.Max(d => d.Fecha) : null
            };
        }
    }
}
