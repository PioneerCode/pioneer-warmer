namespace Pioneer.Warmer
{
    public class Config
    {
        /// <summary>
        /// Resolution of timer in second
        /// </summary>
        public double TimerResolution { get; set; }

        /// <summary>
        /// Valid length of seconds to get request
        /// </summary>
        public double ReponseThreshold { get; set; }

        /// <summary>
        /// Url to warm
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Email to notify if an issue
        /// </summary>
        public string EmailTo { get; set; }

        /// <summary>
        /// Who notification email is from
        /// </summary>
        public string EmailFrom { get; set; }

        /// <summary>
        /// Sending email account username
        /// </summary>
        public string EmailUsername { get; set; }

        /// <summary>
        /// Sending email password
        /// </summary>
        public string EmailPassword { get; set; }

        /// <summary>
        /// Email Host address
        /// </summary>
        public string EmailHost { get; set; }

        /// <summary>
        /// Email Host port
        /// </summary>
        public int EmailHostPort { get; set; }
    }
}
