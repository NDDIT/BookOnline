using NguyenDuyDuong.SachOnline.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Newtonsoft.Json.Linq;
using NguyenDuyDuong.SachOnline.Momo;

namespace NguyenDuyDuong.SachOnline.Controllers
{
    public class GioHangController : Controller
    {
        // Tao doi tuong data chua du lieu tu model dbBansach 43 to.
        dbDataContext data = new dbDataContext("Data Source=MSI;Initial Catalog=SachOnline;Integrated Security=True");

        // Tao gio hang
        public List<Giohang> Laygiohang()
        {
            List<Giohang> lstGiohang = Session["Giohang"] as List<Giohang>;
            if (lstGiohang == null)
            {
                // Neu gio hang chua ton tai thi khoi tao ListGiohang
                lstGiohang = new List<Giohang>();
                Session["Giohang"] = lstGiohang;
            }
            return lstGiohang;
        }
        public ActionResult ThemGiohang(int iMasach, string strURL)
        {
            // Lay ra Session gio hang
            List<Giohang> lstGiohang = Laygiohang();

            // Kiem tra san pham nay da ton tai trong Session["Giohang"] chua
            Giohang sanpham = lstGiohang.Find(n => n.iMasach == iMasach);

            if (sanpham == null)
            {
                // Neu san pham chua ton tai, them moi vao gio hang
                sanpham = new Giohang(iMasach);
                lstGiohang.Add(sanpham);
            }
            else
            {
                // Neu san pham da ton tai, tang so luong len 1
                sanpham.iSoluong++;
            }
            Session["TongSoLuong"] = TongSoLuong();
            // Redirect ve trang URL ban dau
            return Redirect(strURL);
        }
        private int TongSoLuong()
        {
            int iTongSoLuong = 0; // Initialize the total quantity to 0

            // Get the shopping cart from the session
            List<Giohang> lstGiohang = Session["Giohang"] as List<Giohang>;

            if (lstGiohang != null)
            {
                // Calculate the total quantity by summing up the quantities of all items in the cart
                iTongSoLuong = lstGiohang.Sum(n => n.iSoluong);
            }

            return iTongSoLuong;
        }
        private double TongTien()
        {
            double iTongTien = 0; // Initialize the total cost to 0

            // Get the shopping cart from the session
            List<Giohang> lstGiohang = Session["Giohang"] as List<Giohang>;

            if (lstGiohang != null)
            {
                // Calculate the total cost by summing up the dThanhtien property of all items in the cart
                iTongTien = lstGiohang.Sum(n => n.dThanhtien);
            }

            return iTongTien;
        }
        public ActionResult GioHang()
        {
            List<Giohang> lstGiohang = Laygiohang();

            if (lstGiohang.Count == 0)
            {
                // If the shopping cart is empty, redirect to the "Index" action of the "BookStore" controller
                return RedirectToAction("Index", "BookStore");
            }

            // Calculate the total quantity and total cost
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            Session["TongSoLuong"] = TongSoLuong();
            // Return the view with the shopping cart items
            return View(lstGiohang);
        }
        public ActionResult GiohangPartial()
        {
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            Session["TongSoLuong"] = TongSoLuong();
            return PartialView(); 
        }
        public ActionResult XoaGiohang(int iMaSP)
        {
            // Lay gio hang tu Session
            List<Giohang> lstGiohang = Laygiohang();

            // Kiem tra sach da co trong Session["Giohang"]
            Giohang sanpham = lstGiohang.SingleOrDefault(n => n.iMasach == iMaSP);

            // Neu ton tai thi cho sua Soluong
            if (sanpham != null)
            {
                
                lstGiohang.RemoveAll(n => n.iMasach == iMaSP);

                
            }
            else if(lstGiohang.Count == 0)
            {
                Session["TongSoLuong"] = 0;
                return RedirectToAction("Index", "BookStore");

            }

            // Redirect to the "GioHang" action
            return RedirectToAction("GioHang");
        }
        public ActionResult CapnhatGiohang(int iMaSP, FormCollection f)
        {
            // Lay gio hang tu Session
            List<Giohang> lstGiohang = Laygiohang();

            // Kiem tra sach da co trong Session["Giohang"]
            Giohang sanpham = lstGiohang.SingleOrDefault(n => n.iMasach == iMaSP);

            // Neu ton tai thi cho sua Soluong
            if (sanpham != null)
            {
                sanpham.iSoluong = int.Parse(f["txtSoLuong"].ToString());
            }

            // Redirect to the "Giohang" action
            return RedirectToAction("Giohang");
        }
        public ActionResult XoaTatcaGiohang()
        {
            // Lay gio hang tu Session
            List<Giohang> lstGiohang = Laygiohang();

            // Clear the shopping cart
            lstGiohang.Clear();
            Session["TongSoLuong"] = 0;
            return RedirectToAction("Index", "BookStore");
        }
        private void GuiEmailDonHang(string emailKhachHang, DONDATHANG donDatHang, List<Giohang> gioHang)
        {
            try
            {
                // Địa chỉ email của người gửi và người nhận
                string Email = ConfigurationManager.AppSettings["Email"];
                string Password = ConfigurationManager.AppSettings["PasswordEmail"];
                string toEmail = emailKhachHang;

                // Tạo đối tượng MailMessage
                MailMessage message = new MailMessage(Email, toEmail);

                // Tiêu đề email
                message.Subject = "Xác nhận đơn hàng";

                // Nội dung email
                message.Body = $"Xin chào ,cảm ơn bạn đã đặt hàng từ cửa hàng chúng tôi. Dưới đây là chi tiết đơn hàng của bạn:\n\n";

                foreach (var item in gioHang)
                {
                    message.Body += $"Sản phẩm: {item.sTensach}\n";
                    message.Body += $"Số lượng: {item.iSoluong}\n";
                    message.Body += $"Đơn giá: {item.dDongia:C}\n";
                    message.Body += $"Thành tiền: {item.dThanhtien:C}\n\n";
                }

                message.Body += "Chúng tôi rất vui mừng được phục vụ bạn và mong sớm nhìn thấy bạn lần sau.\n\nCảm ơn bạn!\n";
                // Cấu hình đối tượng SmtpClient
                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential(Email, Password); // Thay bằng thông tin tài khoản email của bạn
                smtp.EnableSsl = true;

                // Gửi email
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi khi gửi email (có thể ghi log)
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
            }
        }
        [HttpGet]
        public ActionResult DatHang()
        {
            // Kiem tra dang nhap
            if (Session["Taikhoan"] == null || Session["Taikhoan"].ToString() == "")
                return RedirectToAction("DangNhap", "User", new { strURL = Request.Url.AbsoluteUri });

            if (Session["Giohang"] == null)
                return RedirectToAction("Index", "BookStore");

            // Lay gio hang tu Session
            List<Giohang> lstGiohang = Laygiohang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();

            return View(lstGiohang);
        }
        public ActionResult DatHang(FormCollection collection)
        {
            
            DONDATHANG ddh = new DONDATHANG();

            KHACHHANG kh = (KHACHHANG)Session["Taikhoan"];
            List<Giohang> gh = Laygiohang();
            ddh.MaKH = kh.MaKH;
            ddh.NgayDat = DateTime.Now;
            var ngaygiao = collection["Ngaygiao"];
            ddh.NgayGiao = DateTime.Parse(ngaygiao);
            ddh.TinhTrangGiaoHang = 0;
            ddh.DaThanhToan = false;
            data.DONDATHANGs.InsertOnSubmit(ddh);
            data.SubmitChanges();
            
            // Thêm chi tiết đơn hàng
            foreach (var item in gh)
                {
                    CHITIETDATHANG ctdh = new CHITIETDATHANG();
                        ctdh.MaDonHang = ddh.MaDonHang;
                        ctdh.MaSach = item.iMasach;
                        ctdh.SoLuong = (int)item.iSoluong;
                        ctdh.DonGia = (decimal)item.dDongia;
                        data.CHITIETDATHANGs.InsertOnSubmit(ctdh);
                }
                data.SubmitChanges();
            GuiEmailDonHang(kh.Email, ddh, gh);
            // Xóa giỏ hàng sau khi đặt hàng thành công
            Session["Giohang"] = null;

                // Chuyển hướng đến trang xác nhận đơn hàng
                return RedirectToAction("Xacnhandonhang", "Giohang");           
        }
        public ActionResult Payment()
        {
            List<Giohang> lstGioHang = Laygiohang();
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOCJKB20220928";
            string accessKey = "9RSPhTi8yVQssm5Z";
            string serectkey = "plIPmmBjkkj89fZtFDvrmKO35WCyAMZg";
            string orderInfo = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string returnUrl = "http://tdmusachonline.somee.com/GioHang/XacNhanDonHang";
            string notifyurl = "http://tdmusachonline.somee.com/GioHang/DatHang"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

            string amount = TongTien().ToString();
            string orderid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
        //errorCode = 0 : thanh toán thành công(Request.QueryString["errorCode"])
        //Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
        public ActionResult XacNhanDonHang(Result result)
        {
            //lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
            string rMessage = result.message;
            string rOrderId = result.orderId;
            string rErrorCode = result.errorCode; // = 0: thanh toán thành công
            return View();
        }

        [HttpPost]
        public void SavePayment()
        {
            ////cập nhật dữ liệu vào db
            //DONDATHANG ddh = new DONDATHANG();
            //KHACHHANG kh = (KHACHHANG)Session["TaiKhoan"];
            //List<GioHang> lstGioHang = LayGioHang();
            //ddh.MaKH = kh.MaKH;
            //ddh.NgayDat = DateTime.Now;
            ////var ngayGiao = string.Format("{0:MM/dd/yyyy}", f["NgayGiao"]);
            ////ddh.NgayGiao = DateTime.Parse(ngayGiao);
            //ddh.TinhTrangGiaoHang = 1;
            //ddh.DaThanhToan = false;
            //db.DONDATHANGs.InsertOnSubmit(ddh);
            //db.SubmitChanges();
            //foreach (var item in lstGioHang)
            //{
            //    CHITIETDATHANG ctdh = new CHITIETDATHANG();
            //    ctdh.MaDonHang = ddh.MaDonHang;
            //    ctdh.MaSach = item.iMaSach;
            //    ctdh.SoLuong = item.iSoLuong;
            //    ctdh.DonGia = (decimal)item.dDonGia;
            //    db.CHITIETDATHANGs.InsertOnSubmit(ctdh);
            //}
            //db.SubmitChanges();
            //Session["GioHang"] = null;
        }


    }
}