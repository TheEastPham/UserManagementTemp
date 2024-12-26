using CodeBase.API.Middleware;
using CodeBase.EFCore.Data.DB;
using CodeBase.Model.Setting;
using CodeBase.Utility.Certificates;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

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
    options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("CodeBase.Domain") ?? string.Empty);
});
var certificate = JwtCertificateTools.GetIdentityCertificate(Directory.GetCurrentDirectory(), builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.FromSeconds(30),
            IssuerSigningKey = new X509SecurityKey(certificate),
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hub/baseHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
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

