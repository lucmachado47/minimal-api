using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IAdmServices
    {
        Adm Login(LoginDTO loginDTO);

        Adm Incluir(Adm adm);

        Adm Apagar(Adm adm);

        Adm BuscaPorId(int id);

        List<Adm> Todos(int pagina);
    }
}