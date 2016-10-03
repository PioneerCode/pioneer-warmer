using System.Diagnostics;
using System.Net;
using NLog;
using Pioneer.Warmer.Services;

namespace Pioneer.Warmer
{
    /// <summary>
    /// Warm: Insure website does not go idle.
    /// Make a request to site 
    /// Validate request
    /// Send notification if needed
    /// </summary>
    public class Warmer : IWarmer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;
        private readonly IValidationService _validationService;
        private readonly INotificationService _notificationService;

        public Warmer(Config config, 
            IValidationService validationService, 
            INotificationService notificationService)
        {
            _config = config;
            _validationService = validationService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Logic loop
        /// </summary>
        public void Run()
        {
            Warm();
        }

        /// <summary>
        /// Make a web request to site
        /// </summary>
        private void Warm()
        {
            using (var client = new WebClient())
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var htmlCode = client.DownloadString(_config.Url);
                stopWatch.Stop();

                if (!_validationService.IsValidTimeThreshold(stopWatch.ElapsedMilliseconds))
                {
                    _notificationService.NotifyResponseThresholdExceeded(stopWatch.ElapsedMilliseconds);
                    return;
                }

                if (!_validationService.IsValidResponse(htmlCode))
                {
                    _notificationService.NotifyInvalidReponse(htmlCode);
                    return;
                }
                Logger.Debug("Warming Success - Response time of " + stopWatch.ElapsedMilliseconds / 1000 + " seconds");
            }
        }
    }

    public interface IWarmer
    {
        void Run();
    }
}
