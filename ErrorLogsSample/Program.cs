using System;
using Elastic.Apm.NetCoreAll;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace ErrorLogsSample
{
    public class Program
	{
		public static void Main(string[] args)
		{
            CreateHostBuilder(args).Build().Run();
		}

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithElasticApmCorrelationInfo()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("ServiceName", context.Configuration["ElasticApm:ServiceName"])
                    .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticSearchLogging:Url"]))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = context.Configuration["ElasticSearchLogging:IndexFormat"],
                    ModifyConnectionSettings = x => x.BasicAuthentication(context.Configuration["ElasticSearchLogging:Username"],
                         context.Configuration["ElasticSearchLogging:Password"]),
                    CustomFormatter = new EcsTextFormatter()
                }))
                .UseAllElasticApm()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
	}
}