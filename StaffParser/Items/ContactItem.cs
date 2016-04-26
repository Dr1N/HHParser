using System;

namespace StaffParser
{
    /// <summary>
    /// Контакты
    /// </summary>
    class ContactItem
    {
        public int CvId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return String.Format("CvId: {0, 5}\tName: {1}\tEmail: {2}\tPhone: {3}\tNotes: {4}",
                CvId, Name, Email, Phone, Notes);
        }
    }
}
