using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using simpledrive_client;
using System.Configuration;

namespace LogWriterService
{
    public partial class Service1 : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
        private static System.Timers.Timer timer = new System.Timers.Timer();
        static Configuration config;
        static string config_path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\shared.config";
        static bool sync_in_progress = false;

        public Service1()
        {
            InitializeComponent();
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = config_path;
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("simpleDrive service started");

            timer.Interval = 3000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            timer.Stop();
        }

        public async void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (!sync_in_progress)
            {
                sync_in_progress = true;
                if (!canReadConfig())
                {
                    EventLog.WriteEntry("Config not found");
                    return;
                }

                KeyValueConfigurationElement server = config.AppSettings.Settings["server"];
                KeyValueConfigurationElement user = config.AppSettings.Settings["user"];
                KeyValueConfigurationElement pass = config.AppSettings.Settings["pass"];
                KeyValueConfigurationElement folder = config.AppSettings.Settings["folder"];
                if (server.Value == null && user.Value == null && pass.Value == null && folder.Value == null)
                {
                    EventLog.WriteEntry("Config not complete");
                    return;
                }
                if(!Directory.Exists(folder.Value)) {
                    EventLog.WriteEntry("Sync folder does not exist");
                    //timer.Stop();
                }
                await simpledrive.sync(server.Value, user.Value, pass.Value, folder.Value);
                sync_in_progress = false;
            }
        }

        private bool canReadConfig()
        {
            if (!File.Exists(config_path))
            {
                return false;
            }
            return true;
        }

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };
    }
}
