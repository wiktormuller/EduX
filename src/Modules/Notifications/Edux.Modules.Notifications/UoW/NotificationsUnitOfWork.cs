using Edux.Shared.Infrastructure.Storage.Mongo.Context;
using Edux.Shared.Infrastructure.Storage.Mongo.UoW;
using MongoDB.Driver;

namespace Edux.Modules.Notifications.UoW
{
    internal class NotificationsUnitOfWork : MongoDbUnitOfWork
    {
        public NotificationsUnitOfWork(IMongoClient mongoClient,
            IOperationsContext operationsContext) : base(mongoClient, operationsContext)
        {
        }
    }
}
