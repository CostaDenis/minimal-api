using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities.Interfaces;
using minimal_api.Infrastructure.Db;

namespace minimal_api.Domain.Entities.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;
        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

        public void Create(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
        }

        public void Delete(Vehicle vehicle)
        {
            _context.Vehicles.Remove(vehicle);
            _context.SaveChanges();
        }

        public List<Vehicle> GetAll(int page = 1, string? name = null, string? brand = null)
        {
            var query = _context.Vehicles.AsQueryable();
            int pageSize = 10;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => EF.Functions.Like(x.Name.ToUpper(), $"%{name.ToUpper()}%"));
            }

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return query.ToList();

        }

        public Vehicle? GetById(Guid id)
        {
            return _context.Vehicles.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Update(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
        }
    }
}