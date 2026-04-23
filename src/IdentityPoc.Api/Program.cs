using System.Text.Json.Serialization;
using IdentityPoc.Api.Endpoints;
using IdentityPoc.Api.Infrastructure;
using IdentityPoc.Api.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<JwtTokenOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IdentityStore>();
builder.Services.AddSingleton<IdentityService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new HealthResponse("Healthy", DateTimeOffset.UtcNow)))
    .WithName("HealthCheck");

app.MapAuthenticationEndpoints();
app.MapAuthorizationEndpoints();
app.MapAdminEndpoints();

app.Run();

public sealed record HealthResponse(string Status, DateTimeOffset CheckedAt);
