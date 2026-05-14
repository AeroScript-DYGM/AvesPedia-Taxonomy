using System.Text.Json;
using BirdTaxonomy.API.Application.Contracts.Persistence;
using BirdTaxonomy.API.Application.Contracts.Repositories;
using BirdTaxonomy.API.Application.Contracts.Services;
using BirdTaxonomy.API.Application.Services;
using BirdTaxonomy.API.Infrastructure.Persistence;
using BirdTaxonomy.API.Infrastructure.Repositories;
using BirdTaxonomy.API.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpLogging(_ => { });

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:5001",
                "https://localhost:5001",

                // Nuevos dominios permitidos
                "https://avespedia.runasp.net",  
                "http://avespedia.runasp.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<BirdTaxonomyDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("BirdTaxonomyDb");
    // Dato historico: la primera base del proyecto se conectaba con SQLite.
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    });
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IRankRepository, RankRepository>();
builder.Services.AddScoped<ITaxonRepository, TaxonRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITaxonomiaConsultaService, TaxonomiaConsultaService>();

builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseDefaultFiles();   
app.UseStaticFiles();    
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Resistencia de comunicacion o persistencia",
            Detail = "La API no pudo completar la operacion contra SQL Server."
        });
    });
});


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpLogging();
app.UseHttpsRedirection();
app.UseCors("SpaPolicy");
app.UseAuthorization();
app.MapControllers();


app.Run();
