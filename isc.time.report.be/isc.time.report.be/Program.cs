using isc.time.report.be.api.Extentions;
using isc.time.report.be.api.Security;
using isc.time.report.be.application.IOC;
using isc.time.report.be.domain.Entity.Emails;
using isc.time.report.be.infrastructure.IOC;
using isc.time.report.be.infrastructure.Utils.Secutiry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//var externalConfigPath = @"C:\configs\envvars.json";


//if (File.Exists(externalConfigPath))
//{
//    builder.Configuration.AddJsonFile(externalConfigPath, optional: true, reloadOnChange: true);
//}


var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("OrigenEspecificos", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["JWT:JWTSecretKey"]))
    };
});

builder.Services.AddScoped<CryptoHelper>(); // registro de la depencia 

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Time Report BE", Version = "v1" });
    //options.SwaggerGeneratorOptions.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT aqu�. Ejemplo: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6..."
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddApplication(builder.Configuration);
builder.Services.AddDbConfiguration(builder.Configuration);
builder.Services.AddInfraestructure(builder.Configuration);



builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

builder.Services.Configure<ModuleSecurityOptions>(
    builder.Configuration.GetSection("ModuleSecurity"));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExcepcionHandler();

//app.UseHttpsRedirection();

app.UseCors("OrigenEspecificos");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ModuleAuthorizationMiddleware>();

app.MapControllers();

app.Run();
