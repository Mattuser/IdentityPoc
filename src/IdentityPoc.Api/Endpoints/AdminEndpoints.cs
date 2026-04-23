using IdentityPoc.Api.Contracts;
using IdentityPoc.Api.Infrastructure;

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

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Actor-User-Id", out var values) ||
            !Guid.TryParse(values.FirstOrDefault(), out var actorUserId))
        {
            return Results.Unauthorized();
        }

        return identityService.IsAdmin(actorUserId)
            ? await next(context)
            : Results.StatusCode(StatusCodes.Status403Forbidden);
    }
}
