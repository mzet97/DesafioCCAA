using DesafioCCAA.Api.Common.Api;
using DesafioCCAA.Api.Endpoints.Auth;
using DesafioCCAA.Api.Endpoints.Books;
using DesafioCCAA.Api.Endpoints.Genders;
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

        endpoints.MapGroup("Auth")
           .WithTags("Auth")
           .MapEndpoint<RegisterUserEndpoint>()
           .MapEndpoint<LoginUserEndpoint>()
           .MapEndpoint<ConfirmEmailEndpoint>()
           .MapEndpoint<ForgotPasswordEndpoint>()
           .MapEndpoint<ResetPasswordEndpoint>();

        endpoints.MapGroup("Books")
            .WithTags("Books")
            .RequireAuthorization()
            .MapEndpoint<CreateBookEndpoint>()
            .MapEndpoint<GetByIdBookEndpoint>()
            .MapEndpoint<SearchBookEndpoint>()
            .MapEndpoint<UpdateBookEndpoint>()
            .MapEndpoint<DeletesBookEndpoint>()
            .MapEndpoint<AtivesBookEndpoint>()
            .MapEndpoint<DisablesBookEndpoint>()
            .MapEndpoint<UploadImageEndpoint>()
            .MapEndpoint<GetFileBase64Endpoint>()
            .MapEndpoint<DownloadFileEndpoint>()
            .MapEndpoint<GetByIdUserCreatedBookEndpoint>()
            .MapEndpoint<GenerateBookReportEndpoint>();

        endpoints.MapGroup("Publishers")
            .WithTags("Publishers")
            .RequireAuthorization()
            .MapEndpoint<CreatePublisherEndpoint>()
            .MapEndpoint<GetByIdPublisherEndpoint>()
            .MapEndpoint<SearchPublisherEndpoint>()
            .MapEndpoint<UpdatePublisherEndpoint>()
            .MapEndpoint<DeletesPublisherEndpoint>()
            .MapEndpoint<AtivesPublisherEndpoint>()
            .MapEndpoint<DisablesPublisherEndpoint>();

        endpoints.MapGroup("Genders")
            .WithTags("Genders")
            .RequireAuthorization()
            .MapEndpoint<CreateGenderEndpoint>()
            .MapEndpoint<GetByIdGenderEndpoint>()
            .MapEndpoint<SearchGenderEndpoint>()
            .MapEndpoint<UpdateGenderEndpoint>()
            .MapEndpoint<DeletesGenderEndpoint>()
            .MapEndpoint<AtivesGenderEndpoint>()
            .MapEndpoint<DisablesGenderEndpoint>();
    }


    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}
