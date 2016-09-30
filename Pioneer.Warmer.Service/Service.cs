using System.ComponentModel;
using System.ServiceProcess;
using System.Timers;
using NLog;

namespace Pioneer.Warmer.Service
{
    public partial class Service : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Config _config;
        private readonly IWarmer _warmer;
        private Timer _timer = new Timer();
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        public Service(Config config,
            IWarmer warmer)
        {
            _config = config;
            _warmer = warmer;
            InitializeComponent();
        }

        protected override void OnContinue()
        {
            OnStart(null);
        }

        protected override void OnPause()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Debug("Starting Service.");
            StartTimers();
        }

        protected override void OnStop()
        {
            StopTimers();
        }

        /// <summary>
        /// Start logical loop
        /// </summary>
        private void StartTimers()
        {
            Logger.Debug("Starting Timers.");

            _worker.DoWork += (sender, args) =>
            {
                _warmer.Run();
            };

            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            _timer = new Timer(_config.TimerResolution * 1000)
            {
                AutoReset = true
            };

            _timer.Elapsed += (sender, eventArgs) =>
            {
                if (!_worker.IsBusy)
                {
                    Logger.Debug("Running Worker.");
                    _worker.RunWorkerAsync();
                }
                else
                {
                    Logger.Debug("Worker Busy.");
                }
            };

            _timer.Start();
        }

        /// <summary>
        /// Stop time and clean up worker
        /// </summary>
        private void StopTimers()
        {
            _timer.Stop();
            _worker.Dispose();
        }

        /// <summary>
        /// When work completes, log errors
        /// </summary>
        private static void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.Error(e.Error);
            }
        }
    }
}
