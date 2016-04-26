using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffParser
{
    /// <summary>
    /// Заказ (получаем из бд)
    /// </summary>
    class OrderItem
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int OrderId { get; set; }
        public int CvId { get; set; }
        public bool Ready { get; set; }
        public bool Seen { get; set; }
        public string HhId { get; set; }

        public override string ToString()
        {
            return String.Format("ID: {0, 5}\tClientId: {1, 5}\tOrderId: {2, 5}\tCvId: {3, 5}\tReady: {4}\tSeen: {5}\tHhId: {6}",
                Id, ClientId, OrderId, CvId, Ready, Seen, HhId);
        }
    }
}