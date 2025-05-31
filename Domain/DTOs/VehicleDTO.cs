using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Domain.DTOs
{
    // record -> é uma classe imutável, ou seja, não pode ser alterada após a sua criação
    public record VehicleDTO
    {
        public string Name { get; set; } = default!;
        public string Brand { get; set; } = default!;
        public int Year { get; set; }
    }
}