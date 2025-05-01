using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Api.Endpoints.Publishers;
using Microsoft.AspNetCore.Mvc;

namespace DesafioCCAA.Api.Endpoints;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app
            .MapGroup("");

        endpoints.MapGroup("/")
             .WithTags("Health Check")
             .MapGet("/health-check", async ([FromServices] ILogger<Program> logger) =>
             {
                 logger.LogInformation("Health Check executed.");
                 return Results.Ok(new { message = "OK" });
             });

        endpoints
            .MapGroup("/")
            .WithTags("Cache Redis")
            .MapGet("/cached-endpoint", async (HttpContext context) =>
            {
                context.Response.Headers["Cache-Control"] = "public, max-age=300";
                return Results.Ok(new { Message = "Este é um exemplo de cache", Timestamp = DateTime.UtcNow });
            }).CacheOutput("DefaultPolicy");

        //endpoints.MapGroup("auth")
        //   .WithTags("auth")
        //   .MapEndpoint<RegisterUserEndpoint>()
        //   .MapEndpoint<LoginUserEndpoint>();

        //endpoints.MapGroup("usuarios")
        //   .WithTags("usuarios")
        //   .RequireAuthorization()
        //   .MapEndpoint<CreateUserEndpoint>()
        //   .MapEndpoint<GetByIdUserEndpoint>()
        //   .MapEndpoint<SearchUserEndpoint>()
        //   .MapEndpoint<UpdateUserEndpoint>()
        //   .MapEndpoint<DeleteUserEndpoint>()
        //   .MapEndpoint<UpdatePasswordEndpoint>();

        endpoints.MapGroup("Publishers")
            .WithTags("Publishers")
            //.RequireAuthorization()
            .MapEndpoint<CreatePublisherEndpoint>()
            .MapEndpoint<GetByIdPublisherEndpoint>()
            .MapEndpoint<SearchPublisherEndpoint>()
            .MapEndpoint<UpdatePublisherEndpoint>()
            .MapEndpoint<DeletesPublisherEndpoint>()
            .MapEndpoint<AtivesPublisherEndpoint>()
            .MapEndpoint<DisablesPublisherEndpoint>();
    }


    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}
