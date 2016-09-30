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
            // Make request
            var request = WebRequest.Create(_config.Url);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var response = request.GetResponse();
            stopWatch.Stop();

            // Did it take longer to response then the current expectable threshold?
            _responseTime = stopWatch.ElapsedMilliseconds;
            if (_responseTime > _config.ReponseThreshold * 1000)
            {
                Logger.Debug("Warming Failed - Response time of " +  _responseTime / 1000 + " seconds" + 
                    " seconds was greater then threshold of " + _config.ReponseThreshold + " seconds");
                response.Close();
                return false;
            }

            // Verify response
            var data = response.GetResponseStream();
            if (data == null)
            {
                Logger.Warn("Empty Response");
            }

            Logger.Debug("Warming Succeed");
            response.Close();
            return true;
        }

        /// <summary>
        /// Email on failed "ping"
        /// </summary>
        public void Notify()
        {
            const string body = "<p>Email From: Pioneer Warmer</p>" +
                                "<p>Request: {0}</p>" +
                                "<p>Time to respond: {1}</p>";

            var message = new MailMessage
            {
                From = new MailAddress(_config.EmailFrom, "Pioneer Warmer"),
                Subject = "Pioneer Warmer: Failed - " + _config.Url,
                Body = string.Format(body, _config.Url, _responseTime),
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
