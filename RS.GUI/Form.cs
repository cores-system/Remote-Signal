using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using Microsoft.Win32;
using RS.Shared;

namespace RS.GUI
{
    public partial class Form : System.Windows.Forms.Form
    {
        #region Form
        NotifyIcon ico;
        ComponentResourceManager resources;
        RegistryKey regKay;

        bool NoCancel = true;

        public Form()
        {
            InitializeComponent();
            resources = new ComponentResourceManager(typeof(Form));
            regKay = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");

            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                btn_autorun.Enabled = false;

            if (regKay.GetValue("RemoteSignal") != null)
                btn_autorun.Text = "Удалить с автозапуска";
        }
        #endregion

        #region OnLoad
        async protected override void OnLoad(EventArgs e)
        {
            #region ico
            ico = new NotifyIcon();
            ico.Icon = (Icon)resources.GetObject("$this.Icon");
            ico.Visible = true;

            ico.DoubleClick += delegate (object sender, EventArgs args)
            {
                Show();
                ShowInTaskbar = true;
                WindowState = FormWindowState.Normal;
            };

            var menuStrip = new ContextMenuStrip();
            menuStrip.Items.Add("Выход", null, delegate
            {
                NoCancel = false;
                Close();
            });

            ico.ContextMenuStrip = menuStrip;
            #endregion

            #region RsClient
            tb_log.Text = "Connection..";

            RsClient.OnDestroy += () => { };
            RsClient.OnLog += log => tb_log.Text += log + Environment.NewLine;
            RsClient.OnClearLog += () => tb_log.Text = string.Empty;

            RsClient.BuildOrReBuldHub();
            await RsClient.StartAsync();
            #endregion

            base.OnLoad(e);
        }
        #endregion

        #region OnFormClosing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (NoCancel)
            {
                Hide();
                e.Cancel = true;
            }
            else
            {
                regKay.Close();
                ico.Dispose();
            }

            base.OnFormClosing(e);
        }
        #endregion

        #region AutorunSet
        private void AutorunSet(object sender, EventArgs e)
        {
            try
            {
                if (regKay.GetValue("RemoteSignal") == null)
                {
                    regKay.SetValue("RemoteSignal", Application.ExecutablePath);
                    btn_autorun.Text = "Удалить с автозапуска";
                }
                else
                {
                    regKay.DeleteValue("RemoteSignal");
                    btn_autorun.Text = "Добавить в автозапуск";
                }
            }
            catch { }
        }
        #endregion
    }
}
