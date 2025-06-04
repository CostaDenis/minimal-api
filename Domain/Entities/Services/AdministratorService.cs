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

        public void Create(Administrator administrator)
        {
            _context.Administrators.Add(administrator);
            _context.SaveChanges();
        }

        public List<Administrator> GetAll(int? page = 1)
        {
            var query = _context.Administrators.AsQueryable();
            int pageSize = 10;

            if (page != null)
                query = query.Skip(((int)page - 1) * pageSize).Take(pageSize);

            return query.ToList();

        }

        public Administrator? GetById(Guid id)
        {
            return _context.Administrators.Where(x => x.Id == id).FirstOrDefault();
        }
    }
}