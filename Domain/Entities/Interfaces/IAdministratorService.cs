using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.DTOs;

namespace minimal_api.Domain.Entities.Interfaces
{
    public interface IAdministratorService
    {
        Administrator? Login(LoginDTO loginDTO);
        void Create(Administrator administrator);
        List<Administrator> GetAll(int? page);
        Administrator? GetById(Guid id);
    }
}