using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using NLog;

namespace KustarovBot.Shared
{
    public sealed class MailService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private const string Mail = "mail";

        public async Task SendException(Exception exception)
        {
#if DEBUG
            return;
#endif
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    EnableSsl = true,
                    Port = 587,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(await Configuration.GetValue("gmailAccount"), await Configuration.GetValue("gmailPassword")),
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(await Configuration.GetValue("gmailAccount")),
                    Subject = "KustarovBot exception",
                    Body = exception.ToString(),
                    To = { await Configuration.GetValue("gmailAccount") }
                };

                smtpClient.Send(mail);
                
                Logger.Trace($"[{Mail}] exception message reported.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }
    }
}