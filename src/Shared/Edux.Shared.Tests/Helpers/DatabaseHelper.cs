using Microsoft.EntityFrameworkCore;

namespace Edux.Shared.Tests.Helpers
{
    public static class DatabaseHelper
    {
        public static DbContextOptions<T> GetDbContextOptions<T>() where T : DbContext
        {
            var connectionString = OptionsHelper.GetConfigurationRoot()["sqlserver:connectionString"];

            return new DbContextOptionsBuilder<T>()
                .UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .Options;
        }
    }
}
