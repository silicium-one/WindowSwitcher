using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace WindowSwitcher
{
    public partial class MainForm : Form
    {
        private readonly List<IntPtr> _hWnds = new List<IntPtr>();
        private int _hWndIndex;

        public MainForm()
        {
            InitializeComponent();
        }

        #region старт из коммандной строки
        public MainForm(int interval, List<IntPtr> hWnds) : this()
        {
            timerMain.Interval = interval * 1000;
            _hWnds = hWnds;
            WindowState = FormWindowState.Minimized;
            timerMain.Start();
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
            else
                RefreshList();
        }

        #region usability
        private void textBoxInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBoxInterval_TextChanged(object sender, EventArgs e)
        {
            CheckApplicability();
        }

        private void listViewVisibleWindows_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckApplicability();
        }

        private void CheckApplicability()
        {
            try
            {
                if (int.Parse(textBoxInterval.Text) > 0 && listViewVisibleWindows.SelectedIndices.Count > 0)
                    buttonApply.Enabled = true;
                else
                    buttonApply.Enabled = false;
            }
            catch
            {
                buttonApply.Enabled = false;
            }
            buttonCmd.Enabled = buttonApply.Enabled;
        }

        private void buttonCmd_Click(object sender, EventArgs e)
        {
            string cmdArgs = "\"" + Application.ExecutablePath + "\" " + textBoxInterval.Text;
            cmdArgs = listViewVisibleWindows.SelectedItems.Cast<ListViewItem>().Aggregate(cmdArgs,
                                                                                          (current, item) =>
                                                                                          current +
                                                                                          (" \"" +
                                                                                           WindowsOperationClass.
                                                                                               GetWindowText(
                                                                                                   (IntPtr)item.Tag) +
                                                                                           "\""));
            MessageBox.Show(@"Нажмите OK для копирования командной строки в буфер обмена", @"Командная строка сформирована", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Clipboard.SetText(cmdArgs);
        }
        #endregion

        #region сворачивание в трей
        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }
        #endregion

        #region обслуживание процесса смены окон
        private void buttonApply_Click(object sender, EventArgs e)
        {
            timerMain.Stop();
            _hWndIndex = 0;
            _hWnds.Clear();
            foreach (ListViewItem item in listViewVisibleWindows.SelectedItems)
                _hWnds.Add((IntPtr) item.Tag);
            timerMain.Interval = int.Parse(textBoxInterval.Text)*1000;
            timerMain.Start();
            WindowState = FormWindowState.Minimized;
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            try
            {
                WindowsOperationClass.SetForegroundWindow(_hWnds[_hWndIndex++]);
                _hWndIndex %= _hWnds.Count;
            }
            catch (Exception ex)
            {
               Debug.WriteLine(ex);
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            timerMain.Stop();
            listViewVisibleWindows.Items.Clear();
            CheckApplicability();
            WindowsOperationClass.EnumWindows(delegate(IntPtr hWnd, IntPtr lParam)
            {
                if (WindowsOperationClass.IsWindowVisible(hWnd) && (WindowsOperationClass.GetWindowTextLength(hWnd) != 0) && hWnd != Handle)
                {
                    var listViewItem = new ListViewItem { Text = WindowsOperationClass.GetWindowText(hWnd), Tag = hWnd };
                    listViewVisibleWindows.Items.Add(listViewItem);
                }
                return true;
            }, IntPtr.Zero);
        }
        #endregion
    }
}
