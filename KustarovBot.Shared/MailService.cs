using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using NLog;

namespace KustarovBot.Shared
{
    public sealed class MailService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private const string Mail = "mail";
        
        private SmtpClient _smtpClient;

        public async Task SendException(Exception exception)
        {
            _smtpClient ??= _smtpClient = new SmtpClient("smtp.gmail.com")
            {
                EnableSsl = true,
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    await Configuration.GetValue("gmailAccount"),
                    await Configuration.GetValue("gmailPassword")),
            };
            
            var sender = Assembly.GetExecutingAssembly().FullName;
#if DEBUG
            Logger.Error($"[DEBUG] Exception message from {sender} reported by mail:\n{exception}.");
            return;
#endif
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(await Configuration.GetValue("gmailAccount")),
                    Subject = "KustarovBot exception",
                    Body = $"[{sender}]: {exception}",
                    To = { await Configuration.GetValue("gmailAccount") }
                };

                _smtpClient.Send(mail);
                
                Logger.Trace($"[{Mail}] exception message reported.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }
    }
}