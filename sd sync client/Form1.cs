using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using simpledrive_client;
using System.Configuration;

namespace sd_sync_client
{
    public partial class Form1 : Form
    {
        static string service_name = "simpleDrive Sync Client";
        static string config_path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\shared.config";
        static ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
        static Configuration config;

        static string userdir = "";

        public Form1()
        {
            InitializeComponent();
            fileMap.ExeConfigFilename = config_path;
            config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            if (!canReadConfig()) {
                status.Text = "Config not found";
                blockText(true);
                return;
            }

            // Preset values
            server_input.Text = (config.AppSettings.Settings["server"] != null) ? config.AppSettings.Settings["server"].Value : "";
            user_input.Text = (config.AppSettings.Settings["user"] != null) ? config.AppSettings.Settings["user"].Value : "";
            pass_input.Text = (config.AppSettings.Settings["pass"] != null) ? config.AppSettings.Settings["pass"].Value : "";
            sync_folder.Text = (config.AppSettings.Settings["folder"] != null) ? new FileInfo(config.AppSettings.Settings["folder"].Value).Name : "";

            ServiceController sc = new ServiceController(service_name);
            if(sc == null)
            {
                return;
            }
            if (sc.Status == ServiceControllerStatus.Running)
            {
                blockText(true);
                status.ForeColor = Color.Green;
                status.Text = "Sync service running...";
                button2.Text = "Disconnect";
            }
            else if (sc.Status == ServiceControllerStatus.Stopped)
            {
                button2.Text = "Connect";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!canReadConfig())
            {
                status.Text = "Config not found";
                blockText(true);
                return;
            }
            status.Text = "";
            status.ForeColor = Color.Red;

            ServiceController sc = new ServiceController(service_name);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                status.Text = "Sync service stopped";
                button2.Text = "Connect";
                blockText(false);
            }
            else if (sc.Status == ServiceControllerStatus.Stopped)
            {
                if (server_input.Text == "" || user_input.Text == "" || pass_input.Text == "" || userdir == "")
                {
                    status.Text = "No blank fields!";
                    return;
                }

                blockText(true);
                status.Text = "Connecting...";
                string login = simpledrive.login(server_input.Text, user_input.Text, pass_input.Text);
                if (login == null)
                {
                    status.Text = "Connection error";
                    blockText(false);
                }
                else if (login == "1")
                {
                    var settings = new Dictionary<string, string>
                    {
                        { "server", server_input.Text },
                        { "user", user_input.Text },
                        { "pass", pass_input.Text },
                        { "folder", userdir }
                    };
                    writeSettings(settings);
                    sc.Start();
                    try
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 3000, 0));
                        status.ForeColor = Color.Green;
                        status.Text = "Sync service running...";
                        button2.Text = "Disconnect";
                        simpledrive.create_fav_link(userdir);
                    }
                    catch (System.ServiceProcess.TimeoutException)
                    {
                        status.Text = "Could not start sync service";
                        blockText(false);
                    }
                }
                else
                {
                    status.Text = "Login failed";
                    blockText(false);
                }
            }
        }

        private void browse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                userdir = folderBrowserDialog1.SelectedPath;
                sync_folder.Text = new FileInfo(userdir).Name;
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

        private void writeSettings(Dictionary<string, string> settings)
        {
            foreach (KeyValuePair<string, string> entry in settings)
            {
                config.AppSettings.Settings.Remove(entry.Key);
                config.AppSettings.Settings.Add(entry.Key, entry.Value);
            }
            config.Save(ConfigurationSaveMode.Minimal);
        }

        private void blockText(bool block)
        {
            server_input.ReadOnly = block;
            user_input.ReadOnly = block;
            pass_input.ReadOnly = block;
            if (block)
            {
                server_input.BorderStyle = BorderStyle.None;
                user_input.BorderStyle = BorderStyle.None;
                pass_input.BorderStyle = BorderStyle.None;
            }
            else
            {
                server_input.BorderStyle = BorderStyle.FixedSingle;
                user_input.BorderStyle = BorderStyle.FixedSingle;
                pass_input.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private void server_input_TextChanged(object sender, EventArgs e)
        {

        }

        private void user_input_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void pass_input_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
