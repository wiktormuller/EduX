using FluentValidation;

namespace Edux.Modules.Users.Api.Helpers
{
    internal sealed class ValidationDescriptor
    {
        public required int ArgumentIndex { get; init; }
        public required Type ArgumentType { get; init; }
        public required IValidator Validator { get; init; }
    }
}
