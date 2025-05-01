using Microsoft.AspNetCore.ResponseCompression;

namespace DesafioCCAA.Api.Configuration;

public static class ResponseCompressionConfig
{
    public static IServiceCollection AddApiResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;

            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();

            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
            "application/json",
            "application/xml",
            "text/json",
            "text/css",
            "text/javascript",
            "text/plain",
            "text/html",
            "application/x-javascript",
            "application/javascript",
            "image/svg+xml"
            });
        });


        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Optimal;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Optimal;
        });

        return services;
    }
}

