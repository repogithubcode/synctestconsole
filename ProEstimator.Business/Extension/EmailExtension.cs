using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProEstimator.Business.Extension
{
    public static class EmailExtension
    {
        public static Task SendAsync(this SmtpClient client, MailMessage message)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            Guid sendGuid = Guid.NewGuid();

            SendCompletedEventHandler handler = null;
            handler = (t, u) =>
            {
                if (u.UserState is Guid && ((Guid) u.UserState) == sendGuid)
                {
                    client.SendCompleted -= handler;
                    if (u.Cancelled)
                    {
                        source.SetCanceled();
                    }
                    else if (u.Error != null)
                    {
                        source.SetException(u.Error);
                    }
                    else
                    {
                        source.SetResult(null);
                    }
                }
            };

            client.SendCompleted += handler;
            client.SendAsync(message, sendGuid);
            return source.Task;
        }
    }
}
