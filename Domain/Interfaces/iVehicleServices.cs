using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Migrations;

namespace minimal_api.Domain.Interfaces
{
    public interface IVehicleServices
    {
        List<Vehicle> Todos(int pagina, string nome = null, string marca= null);

        Vehicle BuscaPorId(int id);
        void Incluir(Vehicle veiculo);
        void Atualizar(Vehicle veiculo);
        void Apagar(Vehicle veiculo);
    }
}