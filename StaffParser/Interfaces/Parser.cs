using System.Collections.Generic;

namespace StaffParser
{
    /// <summary>
    /// Парсер контактов
    /// </summary>
    interface IParser
    {
        /// <summary>
        /// Парсим с сайта hh контакты для заказов
        /// Связь по CvId
        /// </summary>
        /// <returns>Список контактов</returns>
        IList<ContactItem> Parse();
    }
}