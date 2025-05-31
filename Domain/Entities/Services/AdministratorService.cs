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
        private readonly AppDbContext _context;
        public AdministratorService(AppDbContext context)
        {
            _context = context;
        }

        public Administrator? Login(LoginDTO loginDTO)
        {
            var adm = _context.Administrators.Where(x => x.Email == loginDTO.Email
                && x.Password == loginDTO.Password).FirstOrDefault();

            return adm;
        }
    }
}