using CodeBase.API.Domain.Model.Setting;
using CodeBase.API.Middleware;
using CodeBase.EFCore.Data.DB;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register controllers
builder.Services.AddControllers();
// Register TelemetryClient
builder.Services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();
builder.Services.AddSingleton<TelemetryClient>();

var applicationSettings = new ApplicationSettings();
builder.Configuration.GetSection("ApplicationSettings").Bind(applicationSettings);
builder.Services.AddSingleton(applicationSettings);
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("CodeBase") ?? string.Empty);
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

