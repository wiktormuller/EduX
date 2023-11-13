namespace Edux.Modules.Users.Application.Graphql.Messaging
{
    public class ReturnedUserMeMessage
    {
        public Guid Id { get; }
        public string Email { get; }

        public ReturnedUserMeMessage(Guid id, string email)
        {
            Id = id;
            Email = email;
        }
    }
}
