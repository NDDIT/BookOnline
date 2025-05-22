using NguyenDuyDuong.SachOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;



namespace NguyenDuyDuong.SachOnline.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        dbDataContext db = new dbDataContext("Data Source=MSI;Initial Catalog=SachOnline;Integrated Security=True");

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Dangky(FormCollection collection, KHACHHANG kh)
        {
            // Lấy giá trị từ Form
            var hoten = collection["HotenKH"];
            var tendn = collection["TenDN"];
            var matkhau = collection["Matkhau"];
            var matkhaunhaplai = collection["Matkhaunhaplai"];
            var diachi = collection["Diachi"];
            var email = collection["Email"];
            var dienthoai = collection["Dienthoai"];
            var ngaysinh = collection["Ngaysinh"];

            // Kiểm tra và xử lý lỗi
            if (string.IsNullOrEmpty(hoten))
            {
                ViewData["Loi1"] = "Họ tên khách hàng không được để trống";
            }

            else if (string.IsNullOrEmpty(tendn))
            {
                ViewData["Loi2"] = "Phải nhập tên đăng nhập";
            }

            else if (string.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi3"] = "Phải nhập mật khẩu";
            }

            else if (string.IsNullOrEmpty(matkhaunhaplai))
            {
                ViewData["Loi4"] = "Phải nhập lại mật khẩu";
            }

            else if (string.IsNullOrEmpty(email))
            {
                ViewData["Loi5"] = "Email không được để trống";
            }

            else if (string.IsNullOrEmpty(dienthoai))
            {
                ViewData["Loi6"] = "Phải nhập số điện thoại";
            }

            // Kiểm tra lỗi
            else if (ViewData.Values.Any(v => !string.IsNullOrEmpty(v as string)))
            {
                return View(); // Hiển thị lại trang đăng ký với thông báo lỗi
            }
            else
            {
                // Gán giá trị cho đối tượng khách hàng (kh)
                kh.HoTen = hoten;
                kh.TaiKhoan = tendn;
                kh.MatKhau = matkhau;
                kh.Email = email;
                kh.DiaChi = diachi;
                kh.DienThoai = dienthoai;
                kh.NgaySinh = DateTime.Parse(ngaysinh);

                // Lưu thông tin khách hàng vào CSDL (db là đối tượng DbContext)
                db.KHACHHANGs.InsertOnSubmit(kh);
                db.SubmitChanges();

                return RedirectToAction("DangNhap"); // Chuyển hướng đến trang đăng nhập
            }
            return this.DangKy();
        }
        [HttpGet]
        public ActionResult DangNhap(string strURL)
        {
            ViewBag.strURL = strURL;
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(FormCollection collection)
        {
            // Lấy giá trị từ Form
            var tendn = collection["TenDN"];
            var matkhau = collection["Matkhau"];
            var strurl = collection["strURL"];

            // Kiểm tra và xử lý lỗi
            if (string.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (string.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                // Gán giá trị cho tài khoản khách hàng (kh) nếu tìm thấy tài khoản
                KHACHHANG kh = db.KHACHHANGs.SingleOrDefault(n => n.TaiKhoan == tendn && n.MatKhau == matkhau);

                if (kh != null)
                {
                    ViewBag.Thongbao = "Chúc mừng đăng nhập thành công";
                    Session["TaiKhoan"] = kh;
                    if (!string.IsNullOrEmpty(strurl))
                    {
                        return Redirect(strurl);
                    }
                    else
                    {
                        // Xử lý khi strURL là null hoặc rỗng
                        // Bạn có thể chọn redirect đến một URL mặc định hoặc thực hiện hành động khác
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }
        public ActionResult LogOut()
        {
            // Xóa thông tin đăng nhập từ session
            Session["TaiKhoan"] = null;

            // Nếu sử dụng ASP.NET Identity, bạn cũng có thể sử dụng:
            // HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            // Chuyển hướng đến trang chủ
            return RedirectToAction("Index", "BookStore");
        }

    }
}