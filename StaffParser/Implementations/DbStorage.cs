using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace StaffParser
{
    /// <summary>
    /// Хранилище в базе MySql
    /// </summary>
    class DbStorage : IStorage
    {
        //Constants
        private readonly string ORDER_ITEMS_TABLE = "hh_orders_items";
        private readonly string CVS_TABLE = "hh_cv_cache";
        private readonly string CONTACTS_TABLE = "hh_cv_contacts";
        private readonly string ORDERS_ITEMS_TABLE = "hh_orders_items";

        //Fields
        private string connString;
        private MySqlConnection conn;
        private MySqlCommand cmd;
        private MySqlDataReader reader;
        private string getOrdersQuery;
        private string insertContactQueryTemplate;
        private string updateOrdersQueryTemplate;

        public DbStorage()
        {
            connString = ConfigurationManager.ConnectionStrings["staff"].ConnectionString;
            getOrdersQuery = String.Format(
                "SELECT {0}.id, {0}.uid, {0}.order_id, {0}.cv_id, {0}.ready, {0}.seen, {1}.hh_id " +
                "FROM {0}, {1} " +
                "WHERE {0}.ready=0 AND {0}.cv_id={1}.id", ORDER_ITEMS_TABLE, CVS_TABLE);

            insertContactQueryTemplate = "INSERT INTO {0} (id, name, email, phone, notes, updated, ready)" +
                " VALUES('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')";

            updateOrdersQueryTemplate = "UPDATE {0} SET ready='1' WHERE cv_id='{1}'";
        }

        //Methods
        /// <summary>
        /// Получить список заказов, данные которых необходимо спарсить
        /// </summary>
        /// <returns>Список заказов (объекты OrderItem)</returns>
        public IList<OrderItem> GetOrders()
        {
            LogWriter.Write("Получаем заказы...");
            IList<OrderItem> result = null;
            try
            {
                LogWriter.Write("Соединение с базой...");
                conn = new MySqlConnection(connString);
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.CommandText = getOrdersQuery;
                LogWriter.Write("Выполнение запроса...");
                reader = cmd.ExecuteReader();
                result = GetOrderItemFromReader(reader);

                LogWriter.Write("Найдено заказов: " + result.Count);
                PrintList(result);
            }
            catch (MySqlException sqlex)
            {
                LogWriter.Write("ERROR! (работа с базой) " + sqlex.Message);
            }
            catch (Exception ex)
            {
                LogWriter.Write("ERROR! " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return result;
        }

        /// <summary>
        /// Записать данные контактов в базу
        /// to-do добавть транзации на обновление двух таблиц
        /// </summary>
        /// <param name="list">Список контактов</param>
        public void WriteContacts(IList<ContactItem> contacts)
        {
            try
            {
                LogWriter.Write("Соединение с базой...");
                conn = new MySqlConnection(connString);
                conn.Open();
                cmd = conn.CreateCommand();
                foreach (ContactItem contact in contacts)
                {
                    try
                    { 
                        string insertQuery = String.Format(insertContactQueryTemplate, 
                            CONTACTS_TABLE, contact.CvId, contact.Name, contact.Email, contact.Phone, contact.Notes, GetTimeStamp(DateTime.Now), 1);
                        LogWriter.Write("Запрос добавления контактов: " + insertQuery);
                        cmd.CommandText = insertQuery;
                        if (cmd.ExecuteNonQuery() != 0)
                        {
                            LogWriter.Write("Добавлены контакты для cv_id = " + contact.CvId);
                            String updateQuery = String.Format(updateOrdersQueryTemplate, 
                                ORDERS_ITEMS_TABLE, contact.CvId);
                            LogWriter.Write("Запрос обновления заказов: " + updateQuery);
                            cmd.CommandText = updateQuery;
                            if (cmd.ExecuteNonQuery() != 0)
                            {
                                LogWriter.Write("Заказ обновлён");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = String.Format("WARNING! Не удалось обновить таблицы [{0}]\n{1}", contact.CvId, ex.Message);
                        LogWriter.Write(message);
                    }
                }
            }
            catch(MySqlException sqlex)
            {
                LogWriter.Write("ERROR! (работа с базой) " + sqlex.Message);
            }
            catch(Exception ex)
            {
                LogWriter.Write("ERROR! " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Получить список заказов из редера базы данных
        /// </summary>
        /// <param name="reader">Реадер запроса получения необходимых данных</param>
        /// <returns>Список заказов (OrderItem объект)</returns>
        private IList<OrderItem> GetOrderItemFromReader(MySqlDataReader reader)
        {
            List<OrderItem> result = new List<OrderItem>();
            while (reader.Read())
            {
                OrderItem orderItem = new OrderItem()
                {
                    Id = Int32.Parse(reader[0].ToString()),
                    ClientId = Int32.Parse(reader[1].ToString()),
                    OrderId = Int32.Parse(reader[2].ToString()),
                    CvId = Int32.Parse(reader[3].ToString()),
                    Ready = Int32.Parse(reader[4].ToString()) == 1 ? true : false,
                    Seen = Boolean.Parse(reader[5].ToString()),
                    HhId = reader[6].ToString()
                };
                result.Add(orderItem);
            }
            return result;
        }

        /// <summary>
        /// Вывод(логирование) данных списка
        /// </summary>
        /// <param name="list">Список</param>
        private void PrintList(IList<OrderItem> list)
        {
            foreach (OrderItem order in list)
            {
                LogWriter.Write(order.ToString());
            }
        }

        private string GetTimeStamp(DateTime now)
        {
            DateTime begin = new DateTime(1970, 1, 1);
            return ((int)(now - begin).TotalSeconds).ToString();
        }

        private void CloseConnection()
        {
            LogWriter.Write("Закрываем соединение...");
            if (reader.IsClosed == false)
            {
                reader.Close();
            }
            conn.Close();
        }
    }
}