using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication().AddBearerToken("my_scheme");
builder.Services.AddHttpClient();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.MapGet("/login", (string username) =>
//{
//    var claimsPrincipal = new ClaimsPrincipal(
//      new ClaimsIdentity(
//        new[] { new Claim(ClaimTypes.Name, username) },
//         "my_scheme"  
//      )
//    );

//    return Results.SignIn(claimsPrincipal, authenticationScheme: "my_scheme");
//});

//app.MapGet("/user", (ClaimsPrincipal user) =>
//{
//    return Results.Ok($"Welcome {user.Identity.Name}!");
//})
//    .RequireAuthorization();

app.Run();

public partial class Program { }
