using Edux.Modules.Users.Application.Contracts.Responses;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Edux.Modules.Users.Application.Graphql.Messaging
{
    public class UsersMessageService
    {
        // In-memory message stream (it's Topic a.k.a Subject)
        private readonly ISubject<ReturnedUserMeMessage> _messageStream = new ReplaySubject<ReturnedUserMeMessage>(1);

        public ReturnedUserMeMessage AddReturnedUserMeMessage(UserMeResponse userMeResponse)
        {
            var message = new ReturnedUserMeMessage(userMeResponse.Id, userMeResponse.Email);

            _messageStream.OnNext(message);
            return message;
        }

        public IObservable<ReturnedUserMeMessage> GetMessages()
        {
            return _messageStream.AsObservable();
        }
    }
}
