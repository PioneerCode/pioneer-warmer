﻿using System;
using System.Net;
using System.Net.Mail;
using NLog;

namespace Pioneer.Warmer.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;

        public NotificationService(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Notify response threshold has been exceeded 
        /// </summary>
        /// <param name="responseTime">Response time in milliseconds</param>
        /// <param name="page">Page in the process of being warmed</param>
        public void NotifyResponseThresholdExceeded(double responseTime, Page page)
        {
            const string body = "<p>Request: {0}</p>" +
                                "<p>Time to respond: {1}</p>";

            SendEmailNotification(new MailMessage
            {
                From = new MailAddress(_config.EmailFrom, "Pioneer Warmer"),
                Subject = "Pioneer Warmer: Request threshold exceeded",
                Body = string.Format(body, page.Url, responseTime / 1000),
                IsBodyHtml = true,
                To =
                {
                    new MailAddress(_config.EmailTo)
                }
            });
        }

        /// <summary>
        /// Notify response threshold has been exceeded 
        /// </summary>
        /// <param name="stream">Response stream - HTML</param>
        /// <param name="page">Page in the process of being warmed</param>
        public void NotifyInvalidReponse(string stream, Page page)
        {
            var body = stream == null ? "<p>NULL Body.</p>" : "<p>Token missing.</p>";

            SendEmailNotification(new MailMessage
            {
                From = new MailAddress(_config.EmailFrom, "Pioneer Warmer"),
                Subject = "Pioneer Warmer: Invalid Response",
                Body = string.Format("<p>Request: {0}</p>" + body, page.Url),
                IsBodyHtml = true,
                To =
                {
                    new MailAddress(_config.EmailTo)
                }
            });
        }

        /// <summary>
        /// Notify we got something besides 200 level response 
        /// </summary>
        /// <param name="url">Paged tested</param>
        public void NotifyFailedReponse(string url)
        {
            SendEmailNotification(new MailMessage
            {
                From = new MailAddress(_config.EmailFrom, "Pioneer Warmer"),
                Subject = "Pioneer Warmer: Failed Response",
                Body = $"<p>Request: {url}</p><p>Most likely 404.</p>",
                IsBodyHtml = true,
                To =
                {
                    new MailAddress(_config.EmailTo)
                }
            });
        }

        /// <summary>
        /// Send pre-configured notification email 
        /// </summary>
        private void SendEmailNotification(MailMessage message)
        {
            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.EnableSsl = false;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(_config.EmailUsername, _config.EmailPassword);
                    smtp.Host = _config.EmailHost;
                    smtp.Port = _config.EmailHostPort;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Send(message);
                    Logger.Trace("Notification Email Sent");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send notification Email: " + ex);
            }
        }
    }

    public interface INotificationService
    {
        void NotifyResponseThresholdExceeded(double responseTime, Page page);
        void NotifyInvalidReponse(string stream, Page page);
        void NotifyFailedReponse(string url);
    }
}
