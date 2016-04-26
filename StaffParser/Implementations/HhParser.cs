using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;

namespace StaffParser
{
    /// <summary>
    /// Парсер hh через браузер
    /// Из-за асихронности выходит "лапша"
    /// Проходим по всем ссылкам в списке заказов - заполняем список контактов
    /// Основное событие - загрузка документа в браузер
    /// </summary>
    class HhParser : IParser
    {
        //Constants
        private readonly string hhUrl = "https://hh.ru/";
        private readonly string nameStart = "itemprop=\"name\">";
        private readonly string nameEnd = "</div>";
        private readonly string phoneStart = "itemprop=\"telephone\">";
        private readonly string phoneEnd = "</span>";
        private readonly string emailStart = "itemprop=\"email\">";
        private readonly string emailEnd = "</a>";

        //Events
        public event Action Ready;

        //Fields
        private WebBrowser webBrowser;
        private IList<OrderItem> orders;
        private IList<ContactItem> contacts;
        private int currentOrder;
        private string currentUrl;
        
        public HhParser(IList<OrderItem> olist, WebBrowser wb)
        {
            orders = olist;
            webBrowser = wb;
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            contacts = new List<ContactItem>();
            currentOrder = 0;
        }

        //Methods
        /// <summary>
        /// Получить список контактов
        /// </summary>
        public IList<ContactItem> Parse()
        {
            return contacts;
        }

        /// <summary>
        /// Запуск парсинга, из-за асинхронной загрузкт страниц в компонент webBrowser
        /// </summary>
        public void RunParse()
        {
            ParceOrder(0);
        }

        private void ParceOrder(int index)
        {
            try
            { 
                LogWriter.Write("Парсим контакты для заказа: " + index + " cv_id: " + orders[index].CvId);
                string url = GetUrlByOrderItem(orders[index]);
                if (!String.IsNullOrEmpty(url))
                {
                    LogWriter.Write("Url страницы: " + url);
                    LogWriter.Write("Загрузка страницы...");
                    currentUrl = url;
                    webBrowser.Url = new Uri(url);
                }
                else
                {
                    LogWriter.Write("WARNING! Не удалось получить страницу для резюме");
                }
            }
            catch (Exception ex)
            {
                LogWriter.Write("ERROR! (Загрузка страницы) " + ex.Message);
            }
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.ToString() != currentUrl)
            {
                return;
            }

            LogWriter.Write("Страница загружена: " + e.Url);

            GetContactData();

            currentOrder++;
            if (currentOrder < orders.Count)
            {
                ParceOrder(currentOrder);
            }
            else
            {
                LogWriter.Write("Контакты загружены: " + contacts.Count);
                PrintContacts();
                webBrowser.DocumentCompleted -= WebBrowser_DocumentCompleted;
                webBrowser.Url = new Uri("https://hh.ru");
                Ready?.Invoke();
            }
        }

        private void PrintContacts()
        {
            foreach (ContactItem contact in contacts)
            {
                LogWriter.Write(contact.ToString());
            }
        }

        /// <summary>
        /// Получить контакты добавить в коллекцию контактов парсера
        /// </summary>
        private void GetContactData()
        {
            try
            {
                Thread.Sleep(2000);
                string doc = webBrowser.DocumentText;
                string name = GetName(doc);
                string phone = GetPhone(doc);
                string email = GetEmail(doc);
                if (String.IsNullOrEmpty(name) == false && String.IsNullOrEmpty(phone) == false && String.IsNullOrEmpty(email) == false)
                {
                    LogWriter.Write("Получено имя: " + name);
                    LogWriter.Write("Получен телефон: " + phone);
                    LogWriter.Write("Получен email: " + email);
                    ContactItem contact = new ContactItem()
                    {
                        CvId = orders[currentOrder].CvId,
                        Name = name,
                        Email = email,
                        Phone = phone,
                        Notes = ""
                    };
                    LogWriter.Write("Контакт получен!");
                    contacts.Add(contact);
                }
                else
                {
                    LogWriter.Write("WARNING! Данные не получены");
                }
            }
            catch (Exception ex)
            {
                LogWriter.Write("ERROR! (Парсинг данных) " + ex.Message);
            }
        }

        private string GetName(string doc)
        {
            return GetSubBetweenStrings(doc, nameStart, nameEnd);
        }

        private string GetPhone(string doc)
        {
            string phone = GetSubBetweenStrings(doc, phoneStart, phoneEnd);
            phone = Regex.Replace(phone, @"\D", "");
            return "+" + phone;
        }

        private string GetEmail(string doc)
        {
            return GetSubBetweenStrings(doc, emailStart, emailEnd);
        }

        /// <summary>
        /// Получить подстроку между строками
        /// </summary>
        /// <param name="src">Исходный документ</param>
        /// <param name="start">Начальная строка</param>
        /// <param name="end">Конечная строка</param>
        /// <returns>Строка между start и end</returns>
        private string GetSubBetweenStrings(string src, string start, string end)
        {
            string result = String.Empty;
            int startIndex = src.IndexOf(start);
            if (startIndex != -1)
            {
                int endIndex = src.IndexOf(end, startIndex);
                result = src.Substring(startIndex + start.Length, endIndex - (startIndex + start.Length));
            }
            else
            {
                LogWriter.Write(String.Format("WARNING! Не удалось найти в документе подстроку между: [{0}] и [{1}]", start, end));
            }
            return result;
        }

        /// <summary>
        /// Сформировать url для заказа
        /// </summary>
        /// <param name="order">Заказ</param>
        /// <returns>Url на HH для заказа</returns>
        private string GetUrlByOrderItem(OrderItem order)
        {
            if (!String.IsNullOrEmpty(order.HhId))
            {
                return hhUrl + "resume/" + order.HhId;
            }
            return String.Empty;
        }
    }
}