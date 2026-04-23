using IdentityPoc.Api.Contracts;
using IdentityPoc.Api.Infrastructure;

namespace IdentityPoc.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static RouteGroupBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", (LoginRequest request, IdentityService identityService) =>
        {
            var authenticatedUser = identityService.Authenticate(request);

            return authenticatedUser is null
                ? Results.Unauthorized()
                : Results.Ok(authenticatedUser);
        });

        group.MapGet("/users/{userId:guid}/profile", (Guid userId, IdentityService identityService) =>
        {
            var profile = identityService.GetProfile(userId);

            return profile is null
                ? Results.NotFound()
                : Results.Ok(profile);
        });

        return group;
    }
}
