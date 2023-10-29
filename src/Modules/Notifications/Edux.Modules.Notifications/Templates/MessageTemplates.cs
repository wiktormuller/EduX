namespace Edux.Modules.Notifications.Templates
{
    internal static class MessageTemplates
    {
        public static string UserSignedUpSubject = @"User {0} has been created.";
        public static string UserSignedUpBody = @"
Hi, it's Edux here.
User with email: '{0}' has been created on: '{1}.'
";
    }
}
