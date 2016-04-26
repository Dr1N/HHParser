using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace StaffParser
{
    public partial class MainForm : Form
    {
        //Fields
        private IStorage storage;
        private HhParser parser;
        private Timer timer;
        private int updateInterval;
        private int currentTime;
        private bool isUpdate;

        public MainForm()
        {
            InitializeComponent();
            numRate_ValueChanged(null, null);
            tbLogs.Clear();
            LogWriter.ClearFile();
            LogWriter.TextBox = tbLogs;
            webBrowser.Url = new Uri("https://hh.ru");

            currentTime = 0;
            isUpdate = false;
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;

            btnStop.Enabled = false;
        }

        //Events Handlers
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = isUpdate;
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Dispose();
            base.OnFormClosed(e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isUpdate)
            {
                return;
            }
            currentTime += timer.Interval;
            int beforeUpdate = updateInterval - currentTime;
            lblTime.Text = "Секунд до обновления: " + (beforeUpdate >= 0 ? (beforeUpdate / 1000).ToString() : "0");
            if (beforeUpdate <= 0)
            {
                UpdateData();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            numRate.Enabled = false;
            cbRun.Enabled = false;
            if (cbRun.Checked == true)
            {
                UpdateData();
            }
            timer.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            numRate.Enabled = true;
            cbRun.Enabled = true;
            lblTime.Text = "Секунд до обновления:";
            timer.Stop();
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            lblUrl.Text = "Url: " + webBrowser.Url;
        }

        private void numRate_ValueChanged(object sender, EventArgs e)
        {
            updateInterval = (int)numRate.Value * 1000;
        }

        //Methods
        private void UpdateData()
        {
            LogWriter.Write(" ---------- ОБНОВЛЯЕМ ДАННЫЕ ---------- ");
            isUpdate = true;
            storage = new DbStorage();
            IList<OrderItem> orders = storage.GetOrders();
            if (orders != null && orders.Count > 0)
            {
                parser = new HhParser(orders, webBrowser);
                parser.Ready += Parser_Ready;
                parser.RunParse();
            }
            else
            {
                currentTime = 0;
                isUpdate = false;
                LogWriter.Write("Нет заказов для парсинга");
            }
        }

        /// <summary>
        /// Обратный вызов - вызывается после получения данных всех заказов
        /// </summary>
        private void Parser_Ready()
        {
            IList<ContactItem> list = parser.Parse();
            storage.WriteContacts(list);
            currentTime = 0;
            isUpdate = false;
        }
    }
}