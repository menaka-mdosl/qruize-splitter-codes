using Amazon.S3;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QruizeMetadataHandler.Core.App
{
    public static class Startup
    {
        public static IServiceProvider Setup()
        {
            //build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            //--- register services ---

            //aws settings
            var awsOptions = configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonSimpleNotificationService>();

            //other services
            services.AddSingleton(configuration);

            //app entry point
            services.AddTransient<SFAppEntryPoint>();
            
            //logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddLambdaLogger(new LambdaLoggerOptions()
                {
                    IncludeEventId = false
                });
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            });

            ServiceProvider = services.BuildServiceProvider();
            return ServiceProvider;
        }

        public static IServiceProvider ServiceProvider { get; private set; }
    }
}
