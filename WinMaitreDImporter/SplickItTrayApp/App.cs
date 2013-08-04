using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WinMaitreDImporter
{
    public class App
    {
        // Define the system tray icon control.
        private NotifyIcon appIcon = new NotifyIcon();
        Icon greenIco = Properties.Resources.AppGreen;
        Icon redIco = Properties.Resources.AppRed;

        // Define the menu.
        private ContextMenu sysTrayMenu = new ContextMenu();
        private MenuItem settings = new MenuItem("Settings...");
        private MenuItem exitApp = new MenuItem("Exit");
        FormSettings frmSettings = new FormSettings();
        
        //private SubscribeThread subscriberThread;
        private System.Threading.Timer timer = null;

        public void Start()
        {
            // Configure the system tray icon.
            appIcon.Icon = redIco;
            appIcon.Text = "Splick·It";

            // Place the menu items in the menu.
            sysTrayMenu.MenuItems.Add(settings);
            sysTrayMenu.MenuItems.Add(exitApp);
            appIcon.ContextMenu = sysTrayMenu;

            // Show the system tray icon.
            appIcon.Visible = true;

            // Attach event handlers.
            exitApp.Click += new EventHandler(ExitApp);
            settings.Click += new EventHandler(settingsClicked);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Filter = "ANSER*.XML";
            watcher.Path = ConfigData.Current.OrderPath;
            watcher.Created += new FileSystemEventHandler(ProcessAnswerFile);
            watcher.Renamed += new RenamedEventHandler(ProcessAnswerFile);
            watcher.EnableRaisingEvents = true;

            TimerCallback timerDelegate = new TimerCallback(GetOrder);
            timer = new System.Threading.Timer(timerDelegate, null, 0, ConfigData.Current.Timeperiod_mSec);
            Logger.Log("=============== Application Started ===============");

            if (String.IsNullOrEmpty(ConfigData.Current.Key))
            {
                Logger.Log("Customer Key Value is Null.");
                settingsClicked(this, null);
            }
        }

        private void ProcessAnswerFile(object sender, FileSystemEventArgs e)
        {
            Logger.Log("Answer file discovered: " + e.FullPath);
            Helper.PostAnswer(e.FullPath);
        }

        private void ExitApp(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void CleanUp(object sender, System.EventArgs e)
        {
            Logger.Log("Exiting Application.");
            frmSettings.Close();
            frmSettings.Dispose();
            appIcon.Visible = false;
            ContextMenu ctxMenu = appIcon.ContextMenu;
            appIcon.ContextMenu = null;
            ctxMenu.Dispose();
            appIcon.Dispose();
        }

        private void settingsClicked(object sender, System.EventArgs e)
        {
            DialogResult dlgResult = frmSettings.ShowDialog();
        }

        public static void Main()
        {
            App app = new App();
            app.Start();
            Application.ApplicationExit += new EventHandler(app.CleanUp);

            // Because no forms are being displayed, you need this 
            // statement to stop the application from automatically ending.
            Application.Run();
        }
      
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void GetOrder(object state)
        {
            if (String.IsNullOrEmpty(ConfigData.Current.Key) ||
                String.IsNullOrEmpty(ConfigData.Current.Server))
            {
                Logger.Log("Customer Key or Server URI null");
                return;
            }

            bool hasOrder = false;
            try
            {
                do
                {
                    bool linkActive = Helper.GetOrders(out hasOrder);

                    if (linkActive)
                    {
                        appIcon.Icon = greenIco;
                        if (hasOrder)
                        {
                            appIcon.Text = "Splick·It Remote Printing - Got Order at " + DateTime.Now.ToString();
                        }
                    }
                    else
                    {
                        appIcon.Icon = redIco;
                        appIcon.Text = "Splick·It: Disconnected (" + DateTime.Now.ToString() + ")";
                    }
                }
                while (hasOrder);

            }
            catch (Exception e)
            {
                if (!String.IsNullOrEmpty(e.Message))
                {
                    Logger.Log("Internal Application Error: " + e.Message);
                }
            }
        }
    }
}



