using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SecureProductApi.Services;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {your JWT token}"
        });

    //options.AddSecurityRequirement(
    //    new OpenApiSecurityRequirement
    //    {
    //        {
    //            new OpenApiSecurityScheme
    //            {
    //                Reference = new OpenApiReference
    //                {
    //                    Type = ReferenceType.SecurityScheme,
    //                    Id = "Bearer"
    //                }
    //            },
    //            Array.Empty<string>()
    //        }
    //    });
});

// JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

// Authorization
builder.Services.AddAuthorization();

// JWT Service
builder.Services.AddScoped<JwtService>();

// Cache
builder.Services.AddMemoryCache();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;

            limiterOptions.Window =
                TimeSpan.FromSeconds(30);

            limiterOptions.QueueLimit = 0;
        });

    options.RejectionStatusCode = 429;
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Rate Limiting
app.UseRateLimiter();

// Authentication
app.UseAuthentication();

// Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();