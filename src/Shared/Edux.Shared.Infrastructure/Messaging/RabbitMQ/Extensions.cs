﻿using Edux.Shared.Abstraction.Messaging;
using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Abstractions.Messaging.Subscribers;
using Edux.Shared.Infrastructure.Initializers;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Connections;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Conventions;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Initializers;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.BackgroundServices;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Channels;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Clients;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Publishers;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Messaging.Subscribers;
using Edux.Shared.Infrastructure.Messaging.RabbitMQ.Serializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Edux.Shared.Infrastructure.Messaging.RabbitMQ
{
    public static class Extensions
    {
        private const string RABBIT_SECTION_NAME = "rabbitmq";

        internal static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            var options = services.GetOptions<RabbitMqOptions>(RABBIT_SECTION_NAME);
            services.AddSingleton(options);

            if (options.HostNames is null || !options.HostNames.Any())
            {
                throw new ArgumentException("RabbitMQ hostnames are not specified.", nameof(options.HostNames));
            }

            services.AddSingleton<ChannelAccessor>();
            services.AddSingleton<IChannelFactory, ChannelFactory>();

            services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
            services.AddSingleton<IBusPublisher, RabbitMqPublisher>();
            services.AddSingleton<IRabbitMqSerializer, SystemTextJsonRabbitMqSerializer>();
            services.AddSingleton<MessageSubscriptionsChannel>();
            services.AddSingleton<IMessageSubscriber, RabbitMqMessageSubscriber>();

            services.AddHostedService<RabbitMqBackgroundService>();

            services.AddSingleton<IConventionsBuilder, ConventionsBuilder>();
            services.AddSingleton<IConventionsProvider, ConventionsProvider>();
            services.AddSingleton<IConventionsRegistry, ConventionsRegistry>();

            services.AddSingleton<IStartupInitializer>(new StartupInitializer());
            services.AddTransient<RabbitMqExchangeInitializer>();

            var connectionFactory = new ConnectionFactory
            {
                Port = options.Port,
                VirtualHost = options.VirtualHost,
                UserName = options.Username,
                Password = options.Password,
                RequestedHeartbeat = options.RequestedHeartbeat,
                RequestedConnectionTimeout = options.RequestedConnectionTimeout,
                SocketReadTimeout = options.SocketReadTimeout,
                SocketWriteTimeout = options.SocketWriteTimeout,
                RequestedChannelMax = options.RequestedChannelMax,
                RequestedFrameMax = options.RequestedFrameMax,
                UseBackgroundThreadsForIO = options.UseBackgroundThreadsForIO,
                DispatchConsumersAsync = true,
                ContinuationTimeout = options.ContinuationTimeout,
                HandshakeContinuationTimeout = options.HandshakeContinuationTimeout,
                NetworkRecoveryInterval = options.NetworkRecoveryInterval,
                Ssl = options.Ssl is null
                    ? new SslOption()
                    : new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath, options.Ssl.Enabled)
            };

            ConfigureSsl(connectionFactory, options);

            var producerProviderName = $"{options.ConnectionName}_producer";
            var producerConnection = connectionFactory.CreateConnection(options.HostNames.ToList(), producerProviderName);

            var consumerProviderName = $"{options.ConnectionName}_consumer";
            var consumerConnection = connectionFactory.CreateConnection(options.HostNames.ToList(), consumerProviderName);

            services.AddSingleton(new ConsumerConnection(consumerConnection));
            services.AddSingleton(new ProducerConnection(producerConnection));

            services.AddInitializerToRegistry<RabbitMqExchangeInitializer>();

            return services;
        }

        private static void ConfigureSsl(ConnectionFactory connectionFactory, RabbitMqOptions options)
        {
            if (options.Ssl is null || string.IsNullOrWhiteSpace(options.Ssl.ServerName))
            {
                connectionFactory.Ssl = new SslOption();
                return;
            }

            connectionFactory.Ssl = new SslOption(options.Ssl.ServerName, options.Ssl.CertificatePath, options.Ssl.Enabled);

            if (string.IsNullOrWhiteSpace(options.Ssl.CaCertificatePath))
            {
                return;
            }

            connectionFactory.Ssl.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                if (chain is null)
                {
                    return false;
                }

                chain = new X509Chain();
                var certificate2 = new X509Certificate2(certificate!);
                var signerCertificate2 = new X509Certificate2(options.Ssl.CaCertificatePath);
                chain.ChainPolicy.ExtraStore.Add(signerCertificate2);
                chain.Build(certificate2);
                var ignoredStatuses = Enumerable.Empty<X509ChainStatusFlags>();
                if (options.Ssl.X509IgnoredStatuses?.Any() is true)
                {
                    ignoredStatuses = options.Ssl.X509IgnoredStatuses
                        .Select(s => Enum.Parse<X509ChainStatusFlags>(s, true));
                }

                var statuses = chain.ChainStatus.ToList();

                var isValid = statuses.All(chainStatus => chainStatus.Status == X509ChainStatusFlags.NoError ||
                    ignoredStatuses.Contains(chainStatus.Status));

                return isValid;
            };
        }

        public static IServiceCollection AddExceptionToMessageMapper<T>(this IServiceCollection services)
            where T : class, IExceptionToMessageMapper
        {
            services.AddSingleton<IExceptionToMessageMapper, T>();
            return services;
        }

        public static IServiceCollection AddExceptionToFailedMessageMapper<T>(this IServiceCollection services)
            where T : class, IExceptionToFailedMessageMapper
        {
            services.AddSingleton<IExceptionToFailedMessageMapper, T>();
            return services;
        }

        public static IMessageSubscriber UseRabbitMq(this IApplicationBuilder app)
        {
            return new RabbitMqMessageSubscriber(
                app.ApplicationServices.GetRequiredService<MessageSubscriptionsChannel>());
        }
    }
}
