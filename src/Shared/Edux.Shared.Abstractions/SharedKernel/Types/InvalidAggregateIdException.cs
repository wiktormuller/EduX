using Edux.Shared.Abstractions.SharedKernel.Exceptions;

namespace Edux.Shared.Abstractions.SharedKernel.Types
{
    public class InvalidAggregateIdException : DomainException
    {
        public override string Code { get; } = "invalid_aggregate_id";

        public InvalidAggregateIdException(AggregateId aggregateId) : base($"Invalid aggregate id: {aggregateId}.")
        {
        }
    }
}
