using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffParser
{
    /// <summary>
    /// Хранлище данных
    /// </summary>
    interface IStorage
    {
        /// <summary>
        /// Получить данные о заказах (таблица hh_orders_items, hh_cvs)
        /// </summary>
        IList<OrderItem> GetOrders();

        /// <summary>
        /// Записать данные о контактах в базу
        /// </summary>
        /// <param name="list">Список контактов</param>
        void WriteContacts(IList<ContactItem> list);
    }
}
