//Minimal API

using MinimalApi.Domain.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//DTO -> Data Transfer Object
app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Password == "123456")
    {
        return Results.Ok("Login aprovado!");
    }
    else
        return Results.Unauthorized();

});


app.Run();
