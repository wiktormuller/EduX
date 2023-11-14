using Edux.Modules.Users.Application.Contracts.Responses;

namespace Edux.Modules.Users.Application.Graphql.Messaging
{
    public interface IUsersMessageService
    {
        ReturnedUserMeMessage AddReturnedUserMeMessage(UserMeResponse userMeResponse);

        IObservable<ReturnedUserMeMessage> GetMessages();
    }
}
