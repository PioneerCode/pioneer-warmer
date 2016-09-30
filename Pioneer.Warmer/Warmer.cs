using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using NLog;

namespace Pioneer.Warmer
{
    /// <summary>
    /// Warm: Insure website does not go idle.
    /// Make a request to site
    /// Measure response time
    /// If over threshold, notify
    /// </summary>
    public class Warmer : IWarmer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;

        /// <summary>
        /// Request response time in milliseconds 
        /// </summary>
        private double _responseTime;

        public Warmer(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Logic loop
        /// </summary>
        public void Run()
        {
            if (Warm()) return;
            Notify();
        }

        /// <summary>
        /// Make a web request to site
        /// </summary>
        private bool Warm()
        {
            using (var client = new WebClient())
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var htmlCode = client.DownloadString(_config.Url);
                stopWatch.Stop();
                _responseTime = stopWatch.ElapsedMilliseconds;

                if (!IsValidTimeThreshold() || !IsValidResponse(htmlCode))
                {
                    return false;
                }
            }

            Logger.Debug("Warming Succeed");
            return true;
        }

        /// <summary>
        /// Did it take longer to response then the current expectable threshold?
        /// </summary>
        private bool IsValidTimeThreshold()
        {
            if (!(_responseTime > _config.ReponseThreshold * 1000)) return true;

            Logger.Debug("Warming Failed - Response time of " + _responseTime / 1000 + " seconds" +
                         " seconds was greater then threshold of " + _config.ReponseThreshold + " seconds");
            return false;
        }

        /// <summary>
        /// Determine if response is valid
        /// </summary>
        private bool IsValidResponse(string stream)
        {
            if (string.IsNullOrEmpty(stream))
            {
                Logger.Warn("Empty Response");
                return false;
            }

            if (_config.Token == null) return true;
            if (stream.Contains(_config.Token)) return true;

            Logger.Warn("Token Missing.");
            return false;
        }

        /// <summary>
        /// Email on failed "ping"
        /// </summary>
        private void Notify()
        {
            const string body = "<p>Email From: Pioneer Warmer</p>" +
                                "<p>Request: {0}</p>" +
                                "<p>Time to respond: {1}</p>";

            var message = new MailMessage
            {
                From = new MailAddress(_config.EmailFrom, "Pioneer Warmer"),
                Subject = "Pioneer Warmer: Failed - " + _config.Url,
                Body = string.Format(body, _config.Url, _responseTime / 1000),
                IsBodyHtml = true,
                To =
                {
                    new MailAddress(_config.EmailTo)
                }
            };

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

    public interface IWarmer
    {
        void Run();
    }
}
