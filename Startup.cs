using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Entities.Interfaces;
using minimal_api.Domain.Entities.Services;
using minimal_api.Domain.Enums;
using minimal_api.Domain.ModelViews;
using minimal_api.Infrastructure.Db;

namespace minimal_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            key = Configuration.GetSection("Jwt")?.ToString() ?? "";
        }

        private string key = string.Empty;

        public IConfiguration Configuration { get; set; } = default!;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(option =>
            {
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        // ValidateAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)
                        ),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };
                });

            services.AddAuthorization();

            services.AddScoped<IAdministratorService, AdministratorService>();
            services.AddScoped<IVehicleService, VehicleService>();

            //Adiciona Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
                {
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Insira o token JWT aqui:"
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                {
                    new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
                }

                    });
                });

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection")!,
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"))
                );
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous();
                #endregion

                #region Administrators
                //DTO -> Data Transfer Object
                //FromBody -> Indica que o objeto será enviado no corpo da requisição
                endpoints.MapPost("/administrator/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
                {
                    var adm = administratorService.Login(loginDTO);
                    if (adm != null)
                    {
                        string token = GenerateTokenJwt(adm);
                        return Results.Ok(new LoggedAdministratorModelView
                        {
                            Id = adm.Id,
                            Email = adm.Email,
                            Profile = adm.Profile,
                            Token = token
                        });
                    }
                    else
                        return Results.Unauthorized();

                }).AllowAnonymous().WithTags("Administrator");


                endpoints.MapGet("/administrator", ([FromQuery] int? page, IAdministratorService administratorService) =>
                {
                    var administratorsModelView = new List<AdministratorModelView>();
                    var administrators = administratorService.GetAll(page);

                    foreach (var adm in administrators)
                    {
                        administratorsModelView.Add(new AdministratorModelView
                        {
                            Id = adm.Id,
                            Email = adm.Email,
                            Profile = adm.Profile
                        });
                    }
                    return Results.Ok(administrators);

                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrator");


                endpoints.MapGet("/administrator/{id}", ([FromRoute] Guid id, IAdministratorService administratorService) =>
                {
                    var administrator = administratorService.GetById(id);

                    if (administrator == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(new AdministratorModelView
                    {
                        Id = administrator.Id,
                        Email = administrator.Email,
                        Profile = administrator.Profile
                    });

                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrator");


                endpoints.MapPost("/administrator", ([FromBody] AdministratorDTO administratorDTO, IAdministratorService administratorService) =>
                {
                    var validation = new ValidationError
                    {
                        Message = new List<string>()
                    };

                    if (string.IsNullOrEmpty(administratorDTO.Email))
                        validation.Message.Add("O email do administrador não pode ser vazio.");

                    if (string.IsNullOrEmpty(administratorDTO.Password))
                        validation.Message.Add("A senha do administrador não pode ser vazia.");

                    if (administratorDTO.Profile == null)
                        validation.Message.Add("O perfil do administrador não pode ser vazio.");

                    if (validation.Message.Count > 0)
                    {
                        return Results.BadRequest(validation);
                    }

                    var administrator = new Administrator
                    {
                        Email = administratorDTO.Email,
                        Password = administratorDTO.Password,
                        Profile = administratorDTO.Profile.ToString() ?? Profile.User.ToString()
                    };

                    administratorService.Create(administrator);

                    return Results.Created($"/administrator/{administrator.Id}", new AdministratorModelView
                    {
                        Id = administrator.Id,
                        Email = administrator.Email,
                        Profile = administrator.Profile
                    });

                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administrator");

                string GenerateTokenJwt(Administrator administrator)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        return string.Empty;
                    }
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>()
    {
        new Claim("Email", administrator.Email),
        new Claim("Profile", administrator.Profile),
        new Claim(ClaimTypes.Role, administrator.Profile),
    };
                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }
                #endregion

                #region Vehicles
                ValidationError ValidationDTO(VehicleDTO vehicleDTO)
                {
                    var messages = new ValidationError
                    {
                        Message = new List<string>()
                    };

                    if (string.IsNullOrEmpty(vehicleDTO.Name))
                        messages.Message.Add("O nome do veículo não pode ser vazio.");


                    if (string.IsNullOrEmpty(vehicleDTO.Brand))
                        messages.Message.Add("A marca do veículo não pode ser vazia.");

                    if (vehicleDTO.Year < 1886)
                        messages.Message.Add("O ano do veículo não pode ser menor que 1886, ano do primeiro carro.");

                    return messages;
                }


                endpoints.MapPost("/vehicle", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
                {
                    var validation = ValidationDTO(vehicleDTO);
                    if (validation.Message.Count > 0)
                    {
                        return Results.BadRequest(validation);
                    }

                    var vehicle = new Vehicle
                    {
                        Name = vehicleDTO.Name,
                        Brand = vehicleDTO.Brand,
                        Year = vehicleDTO.Year
                    };

                    vehicleService.Create(vehicle);

                    return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, User" }).WithTags("Vehicle");

                endpoints.MapGet("/vehicle", ([FromQuery] int? page, IVehicleService vehicleService) =>
                {
                    var vehicles = vehicleService.GetAll(page);

                    return Results.Ok(vehicles);
                }).RequireAuthorization().WithTags("Vehicle");

                endpoints.MapGet("/vehicle/{id}", ([FromRoute] Guid id, IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.GetById(id);

                    if (vehicle == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(vehicle);

                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, User" }).WithTags("Vehicle");

                endpoints.MapPut("/vehicle/{id}", ([FromRoute] Guid id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
                {

                    var validation = ValidationDTO(vehicleDTO);
                    if (validation.Message.Count > 0)
                    {
                        return Results.BadRequest(validation);
                    }

                    var vehicle = vehicleService.GetById(id);

                    if (vehicle == null)
                    {
                        return Results.NotFound();
                    }

                    vehicle.Name = vehicleDTO.Name;
                    vehicle.Brand = vehicleDTO.Brand;
                    vehicle.Year = vehicleDTO.Year;

                    vehicleService.Update(vehicle);

                    return Results.Ok(vehicle);

                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicle");

                endpoints.MapDelete("/vehicle/{id}", ([FromRoute] Guid id, IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.GetById(id);

                    if (vehicle == null)
                    {
                        return Results.NotFound();
                    }
                    vehicleService.Delete(vehicle);

                    return Results.NoContent();

                }).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Vehicle");

                #endregion
            });
        }
    }
}