//Minimal API

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Entities.Interfaces;
using minimal_api.Domain.Entities.Services;
using minimal_api.Domain.ModelViews;
using minimal_api.Infrastructure.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

//Adiciona Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administrators
//DTO -> Data Transfer Object
//FromBody -> Indica que o objeto será enviado no corpo da requisição
app.MapPost("/administrator/login", ([FromBody] LoginDTO loginDTO, IAdministratorService administratorService) =>
{
    if (administratorService.Login(loginDTO) != null)
    {
        return Results.Ok("Login aprovado!");
    }
    else
        return Results.Unauthorized();

}).WithTags("Administrator");
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


app.MapPost("/vehicle", ([FromBody] VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
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
}).WithTags("Vehicle");

app.MapGet("/vehicle", ([FromQuery] int? page, IVehicleService vehicleService) =>
{
    var vehicles = vehicleService.GetAll(page);

    return Results.Ok(vehicles);
}).WithTags("Vehicle");

app.MapGet("/vehicle/{id}", ([FromRoute] Guid id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);

    if (vehicle == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(vehicle);

}).WithTags("Vehicle");

app.MapPut("/vehicle/{id}", ([FromRoute] Guid id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
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

}).WithTags("Vehicle");

app.MapDelete("/vehicle/{id}", ([FromRoute] Guid id, IVehicleService vehicleService) =>
{
    var vehicle = vehicleService.GetById(id);

    if (vehicle == null)
    {
        return Results.NotFound();
    }
    vehicleService.Delete(vehicle);

    return Results.NoContent();

}).WithTags("Vehicle");

#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();


app.Run();
#endregion