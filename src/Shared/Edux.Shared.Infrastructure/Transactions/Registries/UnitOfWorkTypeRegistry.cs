﻿using Edux.Shared.Abstractions.Transactions;

namespace Edux.Shared.Infrastructure.Transactions.Registries
{
    internal sealed class UnitOfWorkTypeRegistry
    {
        private readonly Dictionary<string, Type> _types = new(); // UnitOfWork per module name
        public void Register<T>() where T : IUnitOfWork => _types[GetKey<T>()] = typeof(T);

        public Type? Resolve<T>() => _types.TryGetValue(GetKey<T>(), out var type)
            ? type
            : null;

        private static string GetKey<T>() => $"{typeof(T).GetModuleName()}";
    }
}
