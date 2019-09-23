﻿using System;
using System.IO;
using System.Net.Http;

using Microsoft.Extensions.Configuration;

using Polly;
using Polly.Extensions.Http;

using Bytewizer.Backblaze.Agent;
using Bytewizer.Backblaze.Client;
using Bytewizer.Backblaze.Storage;
using Bytewizer.Backblaze.Adapters;
using System.Net;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up the agent in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the repository agent services to the collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">Delegate to define the configuration.</param>
        public static IBackblazeAgentBuilder AddBackblazeAgent(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var options = configuration.Get<AgentOptions>();

            return AddBackblazeAgent(services, options);
        }

        /// <summary>
        /// Adds the repository agent services to the collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setupBuilder">Delegate to define the configuration.</param>
        public static IBackblazeAgentBuilder AddBackblazeAgent(this IServiceCollection services, Action<AgentOptions> setupBuilder)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupBuilder == null)
                throw new ArgumentNullException(nameof(setupBuilder));

            var options = new AgentOptions();
            setupBuilder(options);

            return AddBackblazeAgent(services, options);
        }

        /// <summary>
        /// Adds the Backblaze client agent services to the collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">The agent options.</param>
        public static IBackblazeAgentBuilder AddBackblazeAgent(this IServiceCollection services, IAgentOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.Validate();

            var policy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => (int)response.StatusCode == 429)
                .WaitAndRetryAsync(6,
                        retryAttempt => PolicyManager.GetSleepDuration(retryAttempt),
                        onRetry: (exception, timeSpan, count, context) =>
                        {
                            Debug.WriteLine($"Status Code: {exception.Result?.StatusCode} Request Message: {exception.Result?.RequestMessage} Retry attempt {count} waiting {timeSpan.TotalSeconds} seconds before next retry.");
                        });

            services.AddSingleton(options);
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddSingleton<IPolicyManager, PolicyManager>();

            services.AddTransient<UserAgentHandler>();
            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(options.Timeout);
            })
            .AddHttpMessageHandler<UserAgentHandler>()
            .SetHandlerLifetime(TimeSpan.FromSeconds(options.HandlerLifetime))
            .AddPolicyHandler(policy);

            services.AddSingleton<IBackblazeStorage, BackblazeStorage>();

            return new BackblazeAgentBuilder(services);
        }


        private static IAsyncPolicy<HttpResponseMessage> RetryPolicy(int retryCount)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => (int)response.StatusCode == 429)
                .Or<IOException>()
                .WaitAndRetryAsync(retryCount,
                    retryAttempt => PolicyManager.GetSleepDuration(retryAttempt));
        }

        //private static IAsyncPolicy<HttpResponseMessage> RetryPolicy(int retryCount)
        //{
        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .Or<IOException>()
        //        .WaitAndRetryAsync(retryCount,
        //            retryAttempt => PolicyManager.GetSleepDuration(retryAttempt));
        //}
    }
}
