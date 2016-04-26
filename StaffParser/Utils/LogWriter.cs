using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace StaffParser
{
    static class LogWriter
    {
        private static string logFile = "parsing.log";
        public static TextBox TextBox = null;

        public static void Write(string message)
        {
            DateTime dt = DateTime.Now;
            string dateTime = dt.ToShortDateString() + " " + dt.ToLongTimeString() + "." + dt.Millisecond.ToString();
#if DEBUG
            Debug.WriteLine(dateTime + "\t" + message);
#endif
            try
            {
                File.AppendAllText(logFile, dateTime + "\t" + message + Environment.NewLine);
            }
            catch (Exception e)
            {
               MessageBox.Show("Не удалось записать в файл лога " + e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
            if (TextBox != null)
            {
                TextBox.AppendText(dateTime + "\t" + message + Environment.NewLine);
            }
        }

        public static void ClearFile()
        {
            File.Delete(logFile);
        }
    }
}