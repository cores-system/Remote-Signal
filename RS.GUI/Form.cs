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
        NotifyIcon _ico;
        ComponentResourceManager _resources;
        RegistryKey _regKay;

        bool _noCancel = true;

        public Form()
        {
            InitializeComponent();
            _resources = new ComponentResourceManager(typeof(Form));

            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                btn_autorun.Enabled = false;
            else
            {
                _regKay = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");

                if (_regKay.GetValue("RemoteSignal") != null)
                {
                    ShowInTaskbar = false;
                    WindowState = FormWindowState.Minimized;
                    btn_autorun.Text = "Удалить с автозапуска";
                }
            }
        }
        #endregion

        #region OnLoad
        async protected override void OnLoad(EventArgs e)
        {
            #region ico
            _ico = new NotifyIcon();
            _ico.Icon = (Icon)_resources.GetObject("$this.Icon");
            _ico.Visible = true;

            _ico.DoubleClick += delegate (object sender, EventArgs args)
            {
                Show();
                ShowInTaskbar = true;
                WindowState = FormWindowState.Normal;
            };

            var menuStrip = new ContextMenuStrip();
            menuStrip.Items.Add("Выход", null, delegate
            {
                _noCancel = false;
                Close();
            });

            _ico.ContextMenuStrip = menuStrip;
            #endregion

            #region RsClient
            tb_log.Text = "Connection..";

            RsClient.OnDestroy += () => { };
            RsClient.OnLog += log => tb_log.Text += log + Environment.NewLine;
            RsClient.OnClearLog += () => tb_log.Text = string.Empty;

            RsClient.BuildOrReBuildHub();
            await RsClient.StartAsync();
            #endregion

            base.OnLoad(e);
        }
        #endregion

        #region OnFormClosing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_noCancel)
            {
                Hide();
                e.Cancel = true;
            }
            else
            {
                _regKay.Close();
                _ico.Dispose();
            }

            base.OnFormClosing(e);
        }
        #endregion

        #region AutorunSet
        private void AutorunSet(object sender, EventArgs e)
        {
            try
            {
                if (_regKay.GetValue("RemoteSignal") == null)
                {
                    _regKay.SetValue("RemoteSignal", Application.ExecutablePath);
                    btn_autorun.Text = "Удалить с автозапуска";
                }
                else
                {
                    _regKay.DeleteValue("RemoteSignal");
                    btn_autorun.Text = "Добавить в автозапуск";
                }
            }
            catch { }
        }
        #endregion
    }
}
