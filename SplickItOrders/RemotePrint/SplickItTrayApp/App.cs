using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SplickItOrdersApp;
using System.Timers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SplickItOrdersApp
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
        Form1 frmSettings = new Form1();
        frmShowMessage frm = new frmShowMessage();
        
        //private SubscribeThread subscriberThread;
        private System.Threading.Timer timer = null;

        CustomerConfig configData;
        
        public void Start()
        {
            configData = Helper.LoadConfigData();

            // Configure the system tray icon.
            appIcon.Icon = redIco;
            appIcon.Text = "Splick·It";

            TimerCallback timerDelegate = new TimerCallback(GetOrder);
            timer = new System.Threading.Timer(timerDelegate, null, 0, configData.Timeperiod_mSec);
            
            // Place the menu items in the menu.
            sysTrayMenu.MenuItems.Add(settings);
            sysTrayMenu.MenuItems.Add(exitApp);
            appIcon.ContextMenu = sysTrayMenu;

            // Show the system tray icon.
            appIcon.Visible = true;

            // Attach event handlers.
            exitApp.Click += new EventHandler(ExitApp);
            settings.Click += new EventHandler(settingsClicked);


            if (configData.Key == "")
            {
                settingsClicked(this, null);
            }

            frm.Show();
        }

        private void ExitApp(object sender, System.EventArgs e)
        {
            frm.Close();
            frm.Dispose();
            frmSettings.Close();
            frmSettings.Dispose();
            appIcon.Visible = false;
            Application.Exit();
        }

        private void settingsClicked(object sender, System.EventArgs e)
        {
            // load the last saved settings
            configData = Helper.LoadConfigData();
            DialogResult dlgResult = frmSettings.ShowDialog();

            // user may have changed settings, load updates.
            configData = Helper.LoadConfigData();
            GetOrder(null);
        }

        public static void Main()
        {
            App app = new App();
            app.Start();

            // Because no forms are being displayed, you need this 
            // statement to stop the application from automatically ending.
            Application.Run();
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void GetOrder(object state)
        {
            if (String.IsNullOrEmpty(configData.Key) ||
                String.IsNullOrEmpty(configData.Server) ||
                String.IsNullOrEmpty(configData.Printer))
                return;

            bool hasOrder = false;
            try
            {
                do
                {
                    string orderTxt;
                    bool linkActive = Helper.GetOrders(configData, out hasOrder, out orderTxt);

                    if (linkActive)
                    {
                        appIcon.Icon = greenIco;
                        if (hasOrder)
                        {
                            appIcon.Text = "Splick·It Remote Printing - Got Order at " + DateTime.Now.ToString();
                            frm.setText(orderTxt);
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
            catch (Exception)
            {
                //
            }
        }
    }
}



