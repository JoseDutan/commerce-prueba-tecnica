using System.Reflection;
using Commerce.Api.Middleware;
using Commerce.Api.Options;
using Commerce.Api.Repositories;
using Commerce.Api.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

const string CorsPolicy = "Frontend";

// Dapper: mapea columnas como pc_processdate a propiedades como PcProcessDate
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    // Errores de validación del modelo con el mismo formato que el middleware global,
    // sin exponer los mensajes internos del deserializador
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = _ =>
        new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Solicitud inválida",
            Detail = "La fecha de proceso es obligatoria y debe tener el formato yyyy-MM-dd.",
        })
        { ContentTypes = { "application/problem+json" } });

// Swagger / OpenAPI, documentado con los comentarios XML del código
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
    // DateOnly se documenta como fecha ISO (yyyy-MM-dd), igual que se serializa en JSON
    options.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
});

// CORS: solo los orígenes configurados (el front Angular en desarrollo)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.Configure<CsvOptions>(builder.Configuration.GetSection(CsvOptions.SectionName));

// Inyección de dependencias por capa
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<ICsvCommerceParser, CsvCommerceParser>();
builder.Services.AddScoped<ICommerceRepository, CommerceRepository>();
builder.Services.AddScoped<ICommerceService, CommerceService>();

var app = builder.Build();

// Primero en el pipeline para capturar los errores de todo lo que viene después
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();
