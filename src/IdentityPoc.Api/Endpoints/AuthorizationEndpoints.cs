using IdentityPoc.Api.Contracts;
using IdentityPoc.Api.Infrastructure;

namespace IdentityPoc.Api.Endpoints;

public static class AuthorizationEndpoints
{
    public static RouteGroupBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authorization")
            .WithTags("Authorization");

        group.MapPost("/check", (AuthorizationRequest request, IdentityService identityService) =>
        {
            var authorization = identityService.Authorize(request);

            return authorization is null
                ? Results.NotFound()
                : Results.Ok(authorization);
        });

        return group;
    }
}
