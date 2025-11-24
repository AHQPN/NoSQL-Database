using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ticket_Booking_System.Models.admin
{
    public class KhachHangThanThiet
    {
        public string CustomerID { get; set; }
        public string Name { get; set; }
        public string PhoneNum { get; set; }
        public decimal TongTienDaChi { get; set; }
        public int SoLanMua { get; set; }
        public int SoVeDaDat { get; set; }
        public int DiemTichLuy { get; set; }
        public string PhanHang { get; set; }
        public string ChuyenThuongXuyen { get; set; }
        public int SoVeChuyen { get; set; }
        public List<string> ChiTietChuyen { get; set; }
        public List<HuyVe> HuyVe { get; set; } = new List<HuyVe>();
        public List<string> HuyVeTrongThang { get; set; } = new List<string>();
    }
    public class HuyVe
    {
        public string TripName { get; set; }
        public int SoVe { get; set; }
        public DateTime CancelDate { get; set; }
    }

}
