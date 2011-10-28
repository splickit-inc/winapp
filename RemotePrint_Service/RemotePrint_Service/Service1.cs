using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Collections;
using System.Timers;
using System.Net;
using System.IO;

using System.Threading;
using ClassLibrary;
using ServiceStack.Redis;


namespace RemotePrint_Service
{
    class WindowsService : ServiceBase
    {
        private System.ComponentModel.IContainer components;
        private System.Timers.Timer timer = null;
        private bool notification = false;
        private SubscribeThread subscriberThread;
        string _redis = "";
        string lastmessage = "";
        string _key = "";
        bool pinged = true;
        String appPath = "";
        public WindowsService()
        {
            InitializeComponent();

            appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            appPath = System.IO.Path.GetDirectoryName(appPath);

            IniFile file = new IniFile(appPath + "/app.ini");
            //Notification
            notification = file.IniReadValue("Settings", "Notification") == "Yes";
            _redis = file.IniReadValue("Settings", "Redis");
            _key = file.IniReadValue("Settings", "Key");
            if (_key != "")
            {
                if (!notification)
                {
                    timer = new System.Timers.Timer(30000);//30 sec
                    timer.Elapsed += new ElapsedEventHandler(this.ServiceTimer_Tick);
                }
                else
                {
                    try
                    {
                        //ping timer
                        timer = new System.Timers.Timer(60*5*1000);//5 minutes
                        timer.Elapsed += new ElapsedEventHandler(this.ServiceTimer_TickPing);


                        subscriberThread = new SubscribeThread(_redis, gotMessage, _key);

                        Thread oThread = new Thread(new ThreadStart(subscriberThread.Start));

                        oThread.Start();



                    }
                    catch (Exception ex)
                    {
                        //no redis
                    }
                }
            }
            this.ServiceName = "SplickItRemotePrint";
            this.EventLog.Source = "SplickItRemotePrint";
            this.EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;

            if (!EventLog.SourceExists("SplickIt"))
                EventLog.CreateEventSource("SplickIt", "Application");
        }

        private void gotMessage(string message)
        {
            //message
            if (message == "ping")
            {
                pinged = true;
            }
            else
                Code.GetOrders(message, appPath);
        }



        private void ServiceTimer_TickPing(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timer.Stop();
            try
            {
                if (!pinged)
                {
                    //we have a problem
                    string message = "Did not get ping back!";
                    if (message != lastmessage)
                        EventLog.WriteEntry("Splickit Print Service", "Ping Error:" + message);
                    lastmessage = message;
     
                }

                RedisClient redisConsumer = new RedisClient(_redis);
                redisConsumer.PublishMessage(_key, "ping");
                redisConsumer.Quit();
                pinged = false;
            }
            catch (Exception ex)
            {
                if (ex.Message != lastmessage)
                    EventLog.WriteEntry("Splickit Print Service", "Ping Error:" + ex.Message);
                lastmessage = ex.Message;
            }
            this.timer.Start();

        }

        private void ServiceTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timer.Stop();
            try
            {
                Code.GetOrders("",appPath);



            }
            catch (Exception ex)
            {
                if (ex.Message != lastmessage)
                    EventLog.WriteEntry("Splickit Print Service", "HTTP Error:" + ex.Message);
                lastmessage = ex.Message;
            }
            this.timer.Start();

        }




        static void Main(System.String[] args)
        {
            // Thread thrd = Thread.CurrentThread;

            //thrd.Priority = ThreadPriority.BelowNormal; 
            ////Utilities.SendMe("Main");


            WindowsService sw = new WindowsService();
            ServiceBase.Run(sw);
        }

        /// <summary>
        /// Dispose of objects that need it here.
        /// </summary>
        /// <param name="disposing">Whether or not disposing is going on.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// OnStart: Put startup code here
        ///  - Start threads, get inital data, etc.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.

            
                timer.AutoReset = true;
                timer.Enabled = true;
                timer.Start();
            

        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.



                timer.AutoReset = false;
                timer.Enabled = false;
            


        }
        protected override void OnPause()
        {

           
                this.timer.Stop();
            
        }

        protected override void OnContinue()
        {
           
                this.timer.Start();
           
        }

        /// <summary>
        /// OnShutdown(): Called when the System is shutting down
        /// - Put code here when you need special handling
        ///   of code that deals with a system shutdown, such
        ///   as saving special data before shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        /// <summary>
        /// OnCustomCommand(): If you need to send a command to your
        ///   service without the need for Remoting or Sockets, use
        ///   this method to do custom methods.
        /// </summary>
        /// <param name="command">Arbitrary Integer between 128 & 256</param>
        protected override void OnCustomCommand(int command)
        {
            //  A custom command can be sent to a service by using this method:
            //#  int command = 128; //Some Arbitrary number between 128 & 256
            //#  ServiceController sc = new ServiceController("NameOfService");
            //#  sc.ExecuteCommand(command);

            base.OnCustomCommand(command);
        }

        /// <summary>
        /// OnPowerEvent(): Useful for detecting power status changes,
        ///   such as going into Suspend mode or Low Battery for laptops.
        /// </summary>
        /// <param name="powerStatus">The Power Broadcase Status (BatteryLow, Suspend, etc.)</param>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// OnSessionChange(): To handle a change event from a Terminal Server session.
        ///   Useful if you need to determine when a user logs in remotely or logs off,
        ///   or when someone logs into the console.
        /// </summary>
        /// <param name="changeDescription"></param>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

        }





    }
}
