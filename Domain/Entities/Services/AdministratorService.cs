using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Entities.Services
{
    public class AdministratorService : IAdministratorService
    {
        private readonly AppDbContext _contexto;
        public AdministratorService(AppDbContext contexto)
        {
            _contexto = contexto;
        }

        public Administrator? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administrators.Where(x => x.Email == loginDTO.Email && x.Password == loginDTO.Password).FirstOrDefault();

            return adm;
        }
    }
}