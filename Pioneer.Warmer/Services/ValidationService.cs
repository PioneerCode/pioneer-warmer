using NLog;

namespace Pioneer.Warmer.Services
{
    public class ValidationService : IValidationService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;

        public ValidationService(Config config)
        {
            _config = config;
        }

        /// <summary>
        /// Did it take longer to response then the current expectable threshold?
        /// </summary>
        /// <param name="responseTime">Response time in milliseconds</param>
        /// <param name="page">Page in the process of being warmed</param>
        public bool IsValidTimeThreshold(double responseTime, Page page)
        {
            if (!(responseTime > page.ReponseThreshold * 1000)) return true;

            Logger.Warn("Warming Failed - Response time of " + responseTime / 1000 + " seconds" +
                         " seconds was greater then threshold of " + page.ReponseThreshold + " seconds");
            return false;
        }

        /// <summary>
        /// Determine if response is valid
        /// </summary>
        /// <param name="stream">Response stream - HTML</param>
        /// <param name="page">Page in the process of being warmed</param>
        public bool IsValidResponse(string stream, Page page)
        {
            if (string.IsNullOrEmpty(stream))
            {
                Logger.Debug("Empty Response : " + page.Url);
                return false;
            }

            if (page.Token == null) return true;
            if (stream.Contains(page.Token)) return true;

            Logger.Debug("Token Missing : " + page.Url);
            return false;
        }
    }

    public interface IValidationService
    {
        bool IsValidTimeThreshold(double responseTime, Page page);
        bool IsValidResponse(string stream, Page page);
    }
}
