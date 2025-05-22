using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NguyenDuyDuong.SachOnline.Models
{
    public class Giohang
    {

        dbDataContext data = new dbDataContext("Data Source=MSI;Initial Catalog=SachOnline;Integrated Security=True");


        public int iMasach { get; set; }
        public string sTensach { get; set; }
        public string sAnhbia { get; set; }
        public double dDongia { get; set; }
        public int iSoluong { get; set; }

        public double dThanhtien
        {
            get { return iSoluong * dDongia; }
        }

        // Khoi tao gio hang theo Masach duoc truyen vao voi Soluong mac dinh la 1
        public Giohang(int Masach)
        {
            iMasach = Masach;
            SACH sach = data.SACHes.Single(n => n.MaSach == iMasach);
            sTensach = sach.TenSach;
            sAnhbia = sach.AnhBia;
            dDongia = double.Parse(sach.GiaBan.ToString());
            iSoluong = 1;
        }
    }

}