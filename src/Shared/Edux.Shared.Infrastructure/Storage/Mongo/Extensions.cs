using Edux.Shared.Infrastructure.Storage.Mongo.Options;
using Microsoft.Extensions.DependencyInjection;
using Edux.Shared.Infrastructure;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Edux.Shared.Infrastructure.Storage.Mongo.Repositories;
using Edux.Shared.Abstractions.SharedKernel.Types;

namespace Edux.Shared.Infrastructure.Storage.Mongo
{
    internal static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {
            var options = services.GetOptions<MongoOptions>("mongo");
            services.AddSingleton(options);

            var mongoClient = new MongoClient(options.ConnectionString);
            var database = mongoClient.GetDatabase(options.Database);

            services.AddSingleton<IMongoClient>(mongoClient);
            services.AddTransient(sp =>
            {
                var options = sp.GetRequiredService<MongoOptions>();
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(options.Database);
            });

            RegisterConventions();

            return services;
        }

        public static IServiceCollection AddMongoRepository<TEntity, TIdentifiable>(this IServiceCollection services, string collectionName)
            where TEntity : IIdentifiable<TIdentifiable>
        {
            services.AddTransient<IMongoRepository<TEntity, TIdentifiable>>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                return new MongoRepository<TEntity, TIdentifiable>(database, collectionName);
            });

            return services;
        }

        private static void RegisterConventions()
        {
            BsonSerializer.RegisterSerializer(typeof(decimal), 
                new DecimalSerializer(BsonType.Decimal128));

            BsonSerializer.RegisterSerializer(typeof(decimal?), 
                new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

            ConventionRegistry.Register("edux", new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            }, _ => true);
        }
    }
}
