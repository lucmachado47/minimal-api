using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Services
{
    public class VehicleServices : IVehicleServices
    {   
        private readonly DatabaseContext _contexto;
        public VehicleServices(DatabaseContext contexto)
        {
            _contexto = contexto;
        }

        public void Apagar(Vehicle veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public void Atualizar(Vehicle veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Vehicle BuscaPorId(int id)
        {   
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Vehicle veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public List<Vehicle> Todos(int pagina, string nome = null, string marca = null)
        {
            var query = _contexto.Veiculos.AsQueryable();
            if(!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome.ToLower()}%"));
            }

            int itensPorPagina = 10;

            query = query.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}