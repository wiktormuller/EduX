namespace Edux.Shared.Abstractions.Kernel.Types
{
    public abstract class AggregateRoot
    {
        public AggregateId Id { get; protected set; }
        public int Version { get; protected set; }
        public IEnumerable<IDomainEvent> Events => _events;
        private List<IDomainEvent> _events = new();

        private bool _versionIncremented;

        protected void AddEvent(IDomainEvent @event)
        {
            if (!_events.Any() && !_versionIncremented)
            {
                Version++;
                _versionIncremented = true;
            }

            _events.Add(@event);
        }

        protected void IncrementVersion()
        {
            if (_versionIncremented)
            {
                return;
            }
            Version++;
            _versionIncremented = false;
        }

        public void ClearEvents() => _events.Clear();
    }
}
