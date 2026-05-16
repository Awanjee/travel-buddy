using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TravelBuddy.Api.Data;
using TravelBuddy.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Travel Buddy API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useSqlite = builder.Configuration.GetValue<bool>("Database:UseSqlite");

builder.Services.AddDbContext<TravelBuddyDbContext>(options =>
{
    if (useSqlite)
        options.UseSqlite(connectionString ?? "Data Source=travelbuddy.db");
    else
        options.UseNpgsql(connectionString);
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-in-production-min-32-chars";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<TravelBuddy.Api.Services.JwtTokenService>();
builder.Services.AddScoped<TravelBuddy.Api.Services.AuthService>();
builder.Services.AddScoped<TravelBuddy.Api.Services.VisaService>();
builder.Services.AddScoped<TravelBuddy.Api.Services.BookingLinkService>();
builder.Services.AddScoped<TravelBuddy.Api.Services.CandidateGeneratorService>();
builder.Services.AddScoped<TravelBuddy.Api.Services.TripService>();
builder.Services.AddScoped<TravelBuddy.Api.Services.CandidateService>();
builder.Services.AddScoped<TravelBuddy.Api.Services.ItineraryService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TravelBuddyDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapTravelBuddyApi();

app.Run();

public partial class Program;
