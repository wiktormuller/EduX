namespace Edux.Shared.Abstractions.Kernel.Types
{
    public record class AggregateId
    {
        public Guid Value { get; }

        public AggregateId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new InvalidAggregateIdException(value);
            }

            Value = value;
        }

        public static implicit operator Guid(AggregateId id)
            => id.Value;

        public static implicit operator AggregateId(Guid id)
            => new(id);

        public override string ToString() => Value.ToString();
    }
}
