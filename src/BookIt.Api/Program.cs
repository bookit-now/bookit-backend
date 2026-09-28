using System.Text;
using BookIt.Api.Errors;
using BookIt.Application;
using BookIt.Application.Authentication;
using BookIt.Infrastructure;
using BookIt.Infrastructure.Authentication;
using BookIt.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LibraryDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'LibraryDatabase' is required. Configure it with user-secrets or environment variables.");
var jwtOptions = LoadJwtOptions(builder.Configuration);
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the access token returned by register or login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            []
        }
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.AccessAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.FindFirst("token_type")?.Value != "access")
                {
                    context.Fail("Only access tokens can authorize API requests.");
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<RegisterMemberHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<RefreshSessionHandler>();
builder.Services.AddScoped<ILibraryFacade, LibraryFacade>();
builder.Services.AddInfrastructure(connectionString, jwtOptions);
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<LibraryDbContext>("postgresql");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "docs/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "docs";
        options.SwaggerEndpoint("/docs/v1/swagger.json", "BookIt API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();

static JwtOptions LoadJwtOptions(IConfiguration configuration)
{
    var options = new JwtOptions
    {
        Issuer = configuration["Jwt:Issuer"] ?? "BookIt",
        AccessAudience = configuration["Jwt:AccessAudience"] ?? "bookit-api",
        RefreshAudience = configuration["Jwt:RefreshAudience"] ?? "bookit-refresh",
        SigningKey = configuration["Jwt:SigningKey"] ?? string.Empty,
        AccessTokenMinutes = configuration.GetValue("Jwt:AccessTokenMinutes", 60),
        RefreshTokenDays = configuration.GetValue("Jwt:RefreshTokenDays", 15)
    };

    if (Encoding.UTF8.GetByteCount(options.SigningKey) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:SigningKey is required and must contain at least 32 bytes. Configure it with user-secrets or environment variables.");
    }

    if (options.AccessTokenMinutes <= 0 || options.RefreshTokenDays <= 0)
    {
        throw new InvalidOperationException("JWT lifetimes must be positive values.");
    }

    return options;
}

public partial class Program;
