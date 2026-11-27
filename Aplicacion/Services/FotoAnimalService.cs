using System;
using System.Collections.Generic;
using System.Text;
using Aplicacion.Interfaces;
using Dominio.Entidades;
using Dominio.Interfaces;

namespace Aplicacion.Services
{
    public class FotoAnimalService : IFotoAnimalService
    {
        private readonly IFotoAnimalRepository _fotoRepository;

        public FotoAnimalService(IFotoAnimalRepository fotoRepository)
        {
            _fotoRepository = fotoRepository;
        }

        public async Task<List<FotoAnimal>> ObtenerTodosAsync()
        {
            return await _fotoRepository.ObtenerTodosAsync();
        }

        public async Task<FotoAnimal?> ObtenerPorIdAsync(int id)
        {
            return await _fotoRepository.ObtenerPorIdAsync(id);
        }

        public async Task AgregarAsync(FotoAnimal foto)
        {
            await _fotoRepository.AgregarAsync(foto);
        }

        public async Task ActualizarAsync(FotoAnimal foto)
        {
            await _fotoRepository.ActualizarAsync(foto);
        }

        public async Task EliminarAsync(int id)
        {
            await _fotoRepository.EliminarAsync(id);
        }
    }
}