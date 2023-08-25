using Edux.Shared.Abstractions.Kernel.Exceptions;

namespace Edux.Shared.Abstractions.Kernel.Types
{
    public class InvalidAggregateIdException : DomainException
    {
        public override string Code { get; } = "invalid_aggregate_id";

        public InvalidAggregateIdException(AggregateId aggregateId) : base($"Invalid aggregate id: {aggregateId}.")
        {
        }
    }
}
