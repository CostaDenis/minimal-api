using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Domain.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}