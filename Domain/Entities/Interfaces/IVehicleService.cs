using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Domain.DTOs;

namespace minimal_api.Domain.Entities.Interfaces
{
    public interface IVehicleService
    {
        List<Vehicle> GetAll(int page = 1, string? name = null, string? brand = null);

        Vehicle? GetById(Guid id);

        void Create(Vehicle vehicle);

        void Update(Vehicle vehicle);

        void Delete(Vehicle vehicle);
    }
}