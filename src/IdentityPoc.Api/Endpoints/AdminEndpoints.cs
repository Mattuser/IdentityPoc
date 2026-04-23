using IdentityPoc.Api.Contracts;
using IdentityPoc.Api.Infrastructure;
using IdentityPoc.Api.Infrastructure.Security;

namespace IdentityPoc.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Administration")
            .AddEndpointFilter(RequireAdmin);

        group.MapGet("/users", (IdentityService identityService) =>
            Results.Ok(identityService.ListUsers()));

        group.MapPost("/users", (CreateUserRequest request, IdentityService identityService) =>
            Results.Created("/api/admin/users", identityService.CreateUser(request)));

        group.MapPost("/users/{userId:guid}/permissions", (
            Guid userId,
            GrantPermissionRequest request,
            IdentityService identityService) =>
        {
            return identityService.GrantPermissionToUser(userId, request.Permission)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapGet("/groups", (IdentityService identityService) =>
            Results.Ok(identityService.ListGroups()));

        group.MapPost("/groups", (CreateGroupRequest request, IdentityService identityService) =>
            Results.Created("/api/admin/groups", identityService.CreateGroup(request)));

        group.MapPost("/groups/{groupId:guid}/users", (
            Guid groupId,
            AddUserToGroupRequest request,
            IdentityService identityService) =>
        {
            return identityService.AddUserToGroup(request.UserId, groupId)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapPost("/groups/{groupId:guid}/permissions", (
            Guid groupId,
            GrantPermissionRequest request,
            IdentityService identityService) =>
        {
            return identityService.GrantPermissionToGroup(groupId, request.Permission)
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }

    private static async ValueTask<object?> RequireAdmin(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var identityService = context.HttpContext.RequestServices.GetRequiredService<IdentityService>();
        var tokenService = context.HttpContext.RequestServices.GetRequiredService<JwtTokenService>();

        if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var values))
        {
            return Results.Unauthorized();
        }

        var authorization = values.FirstOrDefault();

        if (authorization is null ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Unauthorized();
        }

        var validation = tokenService.Validate(authorization["Bearer ".Length..].Trim());

        if (!validation.IsValid || validation.UserId is null)
        {
            return Results.Unauthorized();
        }

        return identityService.IsAdmin(validation.UserId.Value)
            ? await next(context)
            : Results.StatusCode(StatusCodes.Status403Forbidden);
    }
}
