using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using ClassLibrary;
using System.Timers;
using ServiceStack.Redis;
using RemotePrint_Desktop;
using System.Diagnostics;


namespace SplickItTrayApp
{
    public class App2
    {
        // Define the system tray icon control.
        private NotifyIcon appIcon = new NotifyIcon();

        // Define the menu.
        private ContextMenu sysTrayMenu = new ContextMenu();
        private MenuItem settings = new MenuItem("Settings...");
        private MenuItem exitApp = new MenuItem("Exit");
        Form1 frmSettings = new Form1();
        string lastMessage = "";
        frmShowMessage frm = new frmShowMessage();


        private Thread oThread;
        private SubscribeThread subscriberThread;
        private System.Timers.Timer timer = null;
        bool pinged = true;
        string _redis = "";
        string _key = "";
        string appPath = "";
        private void connect()
        {

            if (subscriberThread != null)
            {
                subscriberThread.ok = false;
                oThread.Interrupt();
            }
            // ServiceTimer_TickPing(this, null);
            timer.Start();
            try
            {

                //server
                subscriberThread = new SubscribeThread(_redis, gotMessage, _key);

                oThread = new Thread(new ThreadStart(subscriberThread.Start));

                oThread.Start();


            }
            catch (Exception ex)
            {
                //no redis
            }
        }
        public void Start()
        {

            //load config file
            appPath = Path.GetDirectoryName(Application.CommonAppDataPath);
            IniFile file = new IniFile(appPath + "/app.ini");

            _redis = file.IniReadValue("Settings", "Redis");
            _key = file.IniReadValue("Settings", "Key");
            // Configure the system tray icon.
            Icon ico = new Icon("AppRed.ico");
            appIcon.Icon = ico;
            appIcon.Text = "Splick텶t Remote Printing - Connecting...";

            //ping timer
            timer = new System.Timers.Timer(3000);//ping after 3 seconds
            timer.Elapsed += new ElapsedEventHandler(this.ServiceTimer_TickPing);



            // Place the menu items in the menu.
            sysTrayMenu.MenuItems.Add(settings);
            sysTrayMenu.MenuItems.Add(exitApp);
            appIcon.ContextMenu = sysTrayMenu;

            // Show the system tray icon.
            appIcon.Visible = true;

            // Attach event handlers.
            exitApp.Click += new EventHandler(ExitApp);
            settings.Click += new EventHandler(settingsClicked);


            if (_key != "")
            {

                connect();
            }
            else
            {
                settingsClicked(this, null);
            }
            frmSettings.fromTray = true;

            frm.Show();
            //frm.Hide();
        }


        private void ExitApp(object sender, System.EventArgs e)
        {

            if (subscriberThread != null)
            {
                //subscriberThread.ok = false;
                //  oThread.Interrupt();
                RedisClient redisConsumer = new RedisClient(_redis);
                redisConsumer.PublishMessage(_key, "bye bye");
                redisConsumer.Quit();
                subscriberThread.ok = false;
                oThread.Abort();


            }
            frm.Dispose();

            appIcon.Visible = false;
            Application.Exit();
        }

        private void settingsClicked(object sender, System.EventArgs e)
        {

            frmSettings.fromTray = true;
            if (frmSettings.ShowDialog() == DialogResult.OK)
            {
                appPath = Path.GetDirectoryName(Application.CommonAppDataPath);
                IniFile file = new IniFile(appPath + "/app.ini");

                _redis = file.IniReadValue("Settings", "Redis");
                _key = file.IniReadValue("Settings", "Key");
                connect();
            }
        }





        public static void Main()
        {
            //EventLog.CreateEventSource("SplickIt Remote Print", "SplickIt");
            App2 app = new App2();
            app.Start();

            // Because no forms are being displayed, you need this 
            // statement to stop the application from automatically ending.
            Application.Run();
        }



        private void ServiceTimer_TickPing(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timer.Stop();

            timer.Interval = 60 * 5 * 1000;//5 minutes

            try
            {
                if (!pinged)
                {
                    //we have a problem
                    string message = "Did not get ping back!";

                }

                RedisClient redisConsumer = new RedisClient(_redis);
                redisConsumer.PublishMessage(_key, "ping");
                redisConsumer.Quit();
                pinged = false;
            }
            catch (Exception ex)
            {
                //
            }
            this.timer.Start();

        }


        private void gotMessage(string message)
        {

            try
            {


                if (message == "test")
                {
                    Icon ico = new Icon("AppGreen.ico");
                    appIcon.Icon = ico;
                    appIcon.Text = "Splick텶t Remote Printing - Connected";
                    appIcon.BalloonTipText ="Connected!";
                    appIcon.BalloonTipTitle="Splick텶t Remote Printing";
                    //appIcon.BalloonTipIcon = ico;
                    appIcon.ShowBalloonTip(5000);

                }
                else
                    if (message == "bye bye")
                    {
                        appIcon.Visible = false;
                        Application.Exit();

                    }
                    else
                        //message
                        if (message.StartsWith("Error:"))
                        {
                            // NotifyIcon appIcon = new NotifyIcon();
                            Icon ico = new Icon("AppRed.ico");
                            appIcon.Icon = ico;
                            appIcon.Text = message.Substring(0, message.Length > 60 ? 60 : message.Length);

                        }
                        else
                        {
                            if (message == "ping")
                            {
                                pinged = true;
                                //NotifyIcon appIcon = new NotifyIcon();
                                Icon ico = new Icon("AppGreen.ico");
                                appIcon.Icon = ico;
                                appIcon.Text = "Splick텶t Remote Printing - Connected";


                            }
                            else
                            {
                                string m = Code.GetOrders(message, appPath);
                                // MessageBox.Show(m);
                                //show window on top with message
                                //NotifyIcon appIcon = new NotifyIcon();
                                Icon ico = new Icon("AppGreen.ico");
                                appIcon.Icon = ico;
                                appIcon.Text = "Splick텶t Remote Printing - Got Order at " + DateTime.Now.ToString();

                                frm.setText(m);


                            }
                        }
            }
            catch (Exception ex)
            {
                //if (ex.Message != lastMessage)
                //  EventLog.WriteEntry("SplickIt Remote Printing", "Message Error:" + ex.Message);
                lastMessage = ex.Message;

            }
        }
    }

}



