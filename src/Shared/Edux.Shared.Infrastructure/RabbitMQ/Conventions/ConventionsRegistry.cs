﻿namespace Edux.Shared.Infrastructure.RabbitMQ.Conventions
{
    internal class ConventionsRegistry : IConventionsRegistry // Can be used to reigster custom conventions instead of using MessageAttribute
    {
        private readonly IDictionary<Type, IConventions> _conventions = new Dictionary<Type, IConventions>();

        public void Add<T>(IConventions conventions) => Add(typeof(T), conventions);

        public void Add(Type type, IConventions conventions) => _conventions[type] = conventions;

        public IConventions Get<T>() => Get(typeof(T));

        public IConventions Get(Type type) => _conventions.TryGetValue(type, out var conventions) ? conventions : null;

        public IEnumerable<IConventions> GetAll() => _conventions.Values;
    }
}