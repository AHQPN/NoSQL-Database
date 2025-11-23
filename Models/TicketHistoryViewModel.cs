using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ticket_Booking_System.Models
{
    public class TicketHistoryViewModel
    {
        public string BillID { get; set; }
        public DateTime CreateAt { get; set; }
        public int Quantity { get; set; }
        public double Total { get; set; }

        // ĐÚNG KIỂU ----> Bill.ListItem là List<TicketItem>
        public List<TicketItem> Tickets { get; set; }

        // ĐÚNG KIỂU ----> Bill.TripInfo là TripInfo
        public TripInfo TripInfo { get; set; }

        public string TripID { get; set; }
        public string Status { get; set; }


        // Thêm property này
        public string PaymentStatus { get; set; }

        // Nếu muốn hiển thị chi tiết ghế
        public List<TicketItem> ListItem { get; set; }
    }

}
