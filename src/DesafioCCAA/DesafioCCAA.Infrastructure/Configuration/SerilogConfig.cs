using DesafioCCAA.Shared.Settings;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DesafioCCAA.Infrastructure.Configuration;

public static class SerilogConfig
{
    public static void ConfigureSerilog(this IHostBuilder builder, IConfiguration configuration)
    {
        var elasticSettings = configuration
                .GetSection("ElasticSettings")
                .Get<ElasticSettings>()
                ?? new ElasticSettings();

        builder.UseSerilog((ctx, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Information)
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                .WriteTo.Elasticsearch(new[] { new Uri(elasticSettings.Uri) }, opts =>
                {
                    opts.DataStream = new DataStreamName("logs", elasticSettings.DataSet, "all");
                    opts.BootstrapMethod = BootstrapMethod.Failure;
                    opts.ConfigureChannel = channelOpts =>
                    {
                        channelOpts.BufferOptions = new BufferOptions();
                    };
                }, transport =>
                {
                    transport.Authentication(new BasicAuthentication(elasticSettings.Username, elasticSettings.Password));
                });
        });
    }
}
