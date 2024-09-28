﻿using Amazon.S3;
using Amazon.SimpleNotificationService;
using QruizeBatchReportHandler.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QruizeBatchReportHandler.Core.Data.Model;


namespace QruizeBatchReportHandler.Core.App
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
            //services.Configure<DbConnectionSettings>(configuration.GetConnectionString("DATABASE_SETTINGS"));
            //database service
            services.AddSingleton<IDataService, MongoDataService>();

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
