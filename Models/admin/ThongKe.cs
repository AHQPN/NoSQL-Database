using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ticket_Booking_System.Models
{
    public class ThongKe
    {
        public int TongVeBanTrongThang { get; set; }   
        public decimal TongDoanhThuTrongThang { get; set; }
        public int SoLuongGheConTrong { get; set; }
        public int LoTrinhHoatDong { get; set; }

    }
}