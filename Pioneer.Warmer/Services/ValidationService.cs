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
        public bool IsValidTimeThreshold(double responseTime)
        {
            if (!(responseTime > _config.ReponseThreshold * 1000)) return true;

            Logger.Debug("Warming Failed - Response time of " + responseTime / 1000 + " seconds" +
                         " seconds was greater then threshold of " + _config.ReponseThreshold + " seconds");
            return false;
        }

        /// <summary>
        /// Determine if response is valid
        /// </summary>
        /// <param name="stream">Response stream - HTML</param>
        public bool IsValidResponse(string stream)
        {
            if (string.IsNullOrEmpty(stream))
            {
                Logger.Warn("Empty Response");
                return false;
            }

            if (_config.Token == null) return true;
            if (stream.Contains(_config.Token)) return true;

            Logger.Warn("Token Missing");
            return false;
        }
    }

    public interface IValidationService
    {
        bool IsValidTimeThreshold(double responseTime);
        bool IsValidResponse(string stream);
    }
}
