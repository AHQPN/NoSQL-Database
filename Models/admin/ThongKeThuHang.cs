using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ticket_Booking_System.Models.Admin
{
    public class ThongKeThuHang
    {
        public int ThuHang { get; set; }
        public string LoTrinh { get; set; }
        public int SoVeDaBan { get; set; }
        public decimal DoanhThu { get; set; }
        public double TyLeLapDay { get; set; }
    }
}