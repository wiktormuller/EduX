namespace Edux.Modules.Users.Application.Exceptions
{
    internal class MappingForDomainEventIsNotDefinedException : Shared.Abstractions.SharedKernel.Exceptions.ApplicationException
    {
        public override string Code { get; } = "domain_event_mapping_not_defined";

        public MappingForDomainEventIsNotDefinedException(string domainEventName) 
            : base($"Mapping for domain event named: '{domainEventName}' is not defined")
        {
        }
    }
}
