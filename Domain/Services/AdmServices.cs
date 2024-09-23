using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Services
{
    public class AdmServices : IAdmServices
    {   
        private readonly DatabaseContext _contexto;
        public AdmServices(DatabaseContext contexto)
        {
            _contexto = contexto;
        }

        public Adm Incluir(Adm adm)
        {
            _contexto.Administradores.Add(adm);
            _contexto.SaveChanges();

            return adm;
        }

        public Adm Apagar(Adm adm)
        {
            _contexto.Administradores.Remove(adm);
            _contexto.SaveChanges();

            return adm;
        }

        public Adm BuscaPorId(int id)
        {   
            return _contexto.Administradores.Where(a => a.Id == id).FirstOrDefault();
        }

        public Adm Login(LoginDTO loginDTO)
        {   
            var adm = _contexto.Administradores.Where(
                a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha
            ).FirstOrDefault();
            return adm;
            
        }

        public List<Adm> Todos(int pagina)
        {
            var query = _contexto.Administradores.AsQueryable();

            int itensPorPagina = 10;

            query = query.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }

    }
}