using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace ups_battery_service
{
    public partial class main : ServiceBase
    {
        private PowerLineStatus lastStatus = PowerLineStatus.Online;
        private DateTime lastEventSent = DateTime.Now.AddDays(-1);
        private DateTime timeGoingOffline = DateTime.Now;

        private System.Timers.Timer timer;

        public main()
        {
            InitializeComponent();

            timer = new System.Timers.Timer(Settings.Default.interval_ms);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
         }

       

        protected override void OnStart(string[] args)
        {
            try
            {
                WriteToLog("Service started, UPS Status is " + SystemInformation.PowerStatus.PowerLineStatus, EventLogEntryType.Information, 13374);
            }
            catch(Exception ex)
            {
                WriteToLog("Problem on Start: " + ex.Message, EventLogEntryType.Error, 13378);
            }            
        }

        protected override void OnStop()
        {
            try
            {
                WriteToLog("Service stopped, UPS Status is " + SystemInformation.PowerStatus.PowerLineStatus, EventLogEntryType.Information, 13375);
            }
            catch(Exception ex)
            {
                WriteToLog("Problem on Stop: " + ex.Message, EventLogEntryType.Error, 13379);
            }
        }

        
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {            

            try
            {
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && lastStatus == PowerLineStatus.Online) //OK: no notification
                    return;

                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && lastStatus != PowerLineStatus.Online) //UPS back online again
                    WriteToLog("UPS is back online.", EventLogEntryType.Information, 13371);

                if (SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online && lastStatus != PowerLineStatus.Online && lastEventSent.AddMinutes(Settings.Default.time_reminder_min) < DateTime.Now) //UPS offline for a longer time
                    WriteToLog("UPS is still offline.", EventLogEntryType.Error, 13372);

                if (SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online && lastStatus == PowerLineStatus.Online) //UPS just went offline
                {
                    WriteToLog("UPS went from online to offline.", EventLogEntryType.Error, 13373);
                    timeGoingOffline = DateTime.Now;
                }

                if (SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Online && timeGoingOffline.AddMinutes(Settings.Default.time_to_wait_before_shutdown_eventlog_entry_min) < DateTime.Now)
                    WriteToLog("UPS is still offline (" + Settings.Default.time_to_wait_before_shutdown_eventlog_entry_min + " min). You should shut down your system.", EventLogEntryType.Error, 13380);

                lastStatus = SystemInformation.PowerStatus.PowerLineStatus;
            }
            catch (Exception ex)
            {
                WriteToLog("Exception in Timer: \r\n" + ex.Message, EventLogEntryType.Error, 13377);
            }


        }

        private void WriteToLog(String text, EventLogEntryType type, int eventid)
        {


            String addonText = "";
            try
            {
                addonText += "\r\nChargeStatus:" + SystemInformation.PowerStatus.BatteryChargeStatus;
                addonText += "\r\nBatteryFullLifetime:" + SystemInformation.PowerStatus.BatteryFullLifetime.ToString();
                addonText += "\r\nBatteryLifePercent:" + SystemInformation.PowerStatus.BatteryLifePercent.ToString();
                addonText += "\r\nBatteryLifeRemaining:" + SystemInformation.PowerStatus.BatteryLifeRemaining.ToString();
                addonText += "\r\nPowerLineStatus:" + SystemInformation.PowerStatus.PowerLineStatus;
            }
            catch (Exception ex)
            {
                addonText += "Error retrieving values for addonText:\r\n" + ex.Message;
            }

            foreach (String server in Settings.Default.servers)
            {
                eventLog.MachineName = server;
                try
                {
                    eventLog.WriteEntry(text + "\r\n\r\n" + addonText, type, eventid);
                }
                catch (Exception ex)
                {
                    eventLog.MachineName = ".";
                    eventLog.WriteEntry("Error writing EventLog to " + server + "\r\n\r\nException: " + ex.Message, EventLogEntryType.Error, 13376);
                }
            }

            lastEventSent = DateTime.Now;
        }


       
    }
}
