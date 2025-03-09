using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TechAid.Data;
using TechAid.Interface;
using TechAid.MiddleWare;
using TechAid.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add CORS services (allowing any origin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()   // Allow any HTTP method (GET, POST, etc.)
              .AllowAnyHeader()
               .AllowCredentials();  // Allow any headers
    });
});

builder.Services.AddControllers();

// Add Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TechAid.Api", Version = "v1" });

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", securitySchema);

    var securityRequirement = new OpenApiSecurityRequirement();
    securityRequirement.Add(securitySchema, new[] { "Bearer" });
    c.AddSecurityRequirement(securityRequirement);
});

// Add scoped services
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IEmailService, EmailService>();


// Add JWT authentication services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Ensures expired tokens are rejected immediately
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])
            )
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                    context.Response.StatusCode = 401;
                }
                return Task.CompletedTask;
            }
        };
    });

// Add database context (Entity Framework)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();

// Enable CORS globally to allow any origin
app.UseCors("AllowAny");  // Apply the "AllowAny" CORS policy

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();  // Add authorization middleware

app.MapControllers();

app.Run();
