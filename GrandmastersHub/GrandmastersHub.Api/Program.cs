using System.Security.Claims;
using System.Text;
using GrandmastersHub.Application.Interfaces;
using GrandmastersHub.Application.Services;
using GrandmastersHub.Api.Middleware;
using GrandmastersHub.Domain.Interfaces;
using GrandmastersHub.Infrastructure.Data;
using GrandmastersHub.Infrastructure.Repositories;
using GrandmastersHub.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
builder.Services.AddDbContext<GrandmastersDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<GrandmastersHub.Domain.Entities.User>, PasswordHasher<GrandmastersHub.Domain.Entities.User>>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AllowFrontend", policy =>
    policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.AddOptions<JwtOptions>().Bind(jwtSection)
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required.")
    .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey ?? string.Empty) >= 32, "Jwt:SigningKey must contain at least 32 bytes.")
    .Validate(o => o.ExpiryMinutes is > 0 and <= 1440, "Jwt:ExpiryMinutes must be between 1 and 1440.")
    .ValidateOnStart();

var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException("The Jwt configuration section is required.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey ?? string.Empty));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true, ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = signingKey,
        ValidateLifetime = true, ClockSkew = TimeSpan.Zero, RoleClaimType = ClaimTypes.Role
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply the checked-in EF Core migrations automatically for local development.
// This keeps the database schema in sync with the source-controlled model.
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<GrandmastersDbContext>();
    await database.Database.MigrateAsync();
}
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseMiddleware<GlobalErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
