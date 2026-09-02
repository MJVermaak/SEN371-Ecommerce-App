using System.Security.Claims;
using System.Text;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Application.Services;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using GrandmastersHub.Infrastructure.Repositories;
using GrandmastersHub.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Swagger and OpenAPI for endpoint testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseRouting();

// Middleware hooks for Auth (Person 3) and Error Handling (Person 4)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();