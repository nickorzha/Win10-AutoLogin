using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace AutoLogin
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        ~Service1()
        {
            //Do this during application close to avoid handle leak
            SystemEvents.SessionSwitch -= new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }


        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            WriteTextToFile("sytem: is locked");

            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    break;

                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:

                    break;
            }
        }


        protected override void OnStart(string[] args)
        {
            WriteTextToFile("Service started at " + DateTime.Now);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 3000; //time interval in milliseconds (10Sec) 
            timer.Enabled = true;
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }
        protected override void OnStop()
        {
            WriteTextToFile("Service stopped at " + DateTime.Now);
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (var key = root.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    try
                    {
                        //HttpClient client = new HttpClient();
                        //client.BaseAddress = new Uri("https://vps.lakemon.com/wifisso.com/api.php?action=get_UUID&hdserial=1");
                        //// Add an Accept header for JSON format.
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        //// List all Names.
                        //HttpResponseMessage response = client.GetAsync("api/Values").Result;  // Blocking call!
                        //if (response.IsSuccessStatusCode)
                        //{
                        //    string credentials = response.Content.ReadAsStringAsync().Result;
                        //    object jsonResult = JsonConvert.DeserializeObject<object>(credentials);

                        //}
                        //else
                        //{
                        //    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                        //}
                        key.SetValue("DefaultUserName", "ryanerb", RegistryValueKind.String);
                        key.SetValue("DefaultPassword", "password", RegistryValueKind.String);
                        key.SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
                        WriteTextToFile(key.GetValue("AutoAdminLogon").ToString()+ key.GetValue("DefaultPassword").ToString()+ key.GetValue("DefaultUserName").ToString());
                    }
                    catch (Exception ex)
                    {
                        WriteTextToFile("error:" + ex.ToString());
                    }
                }
            }

        }

        static void WriteTextToFile(string Message)
        {
            string checkPath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile";
            if (!Directory.Exists(checkPath))
            {
                Directory.CreateDirectory(checkPath);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\LogsFile\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }

            }
        }
    
    
    
    }
}
