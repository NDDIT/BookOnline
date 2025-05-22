using HtmlAgilityPack;
using IBook.Content.module;
using NguyenDuyDuong.SachOnline.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Text;
using System.Web.Security;
using System.Security.Cryptography;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;


namespace NguyenDuyDuong.SachOnline.Areas.Admin.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private dbDataContext db = new dbDataContext("Data Source=MSI;Initial Catalog=SachOnline;Integrated Security=True");

        // GET: Admin
        //[Authorize]
        public ActionResult Statistics()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(FormCollection collection)
        {
            var user = collection["username"];
            var password = collection["pass"];
           

            // Kiểm tra và xử lý lỗi
            if (string.IsNullOrEmpty(user))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (string.IsNullOrEmpty(password))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                // Gán giá trị cho tài khoản khách hàng (kh) nếu tìm thấy tài khoản
                ADMIN ad = db.ADMINs.SingleOrDefault(n => n.TenDN == user && n.MatKhau == password);

                if (ad != null)
                {
                    ViewBag.Thongbao = "Chúc mừng đăng nhập thành công";

                    FormsAuthentication.SetAuthCookie(ad.TenDN, false);

                    return RedirectToAction("Statistics", "Admin");
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login"); 
        }

        [HttpGet]
        public ActionResult BookManagement(int? page)
        {
            //var book = db.SACHes.ToList();

            int pageNumber = (page ?? 1);
            int pageSize = 5;

            var book = db.SACHes.ToList().OrderBy(n => n.MaSach).ToPagedList(pageNumber, pageSize);

            return View(book);
        }
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            return View();
        }
        [HttpPost]

        [ValidateInput(false)]

        public ActionResult Create(SACH sach, FormCollection f, HttpPostedFileBase fFileUpload)

        {

            //Đưa dữ liệu vào DropDown
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

            if (fFileUpload == null)

            {

                //Nội dung thông báo yêu cầu chọn ảnh bìa

                ViewBag.ThongBao = "Hãy chọn ảnh bìa.";

                //Lưu thông tin đề khi load lại trang do yêu cầu chọn ảnh bìa sẽ hiển thị các thông tin này lên trang

                ViewBag.TenSach = f["sTenSach"];
                string sanitizedDescription = SanitizeHtml(f["sMoTa"]);


                ViewBag.MoTa = f["sMoTa"];

                ViewBag.SoLuong = int.Parse(f["iSoLuong"]);

                ViewBag.GiaBan = decimal.Parse(f["mGiaBan"]);

                ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", int.Parse(f["MaCD"]));

                ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", int.Parse(f["MaNXB"]));
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                { //Lấy tên file (Khai báo thư viện: System.IO)
                    var sFileName = Path.GetFileName(fFileUpload.FileName);

                    //Lấy đường dẫn lưu file
                    var path = Path.Combine(Server.MapPath("~/Images/LoadImage"), sFileName);

                    //Kiểm tra ảnh bìa đã tồn tại chưa để lưu lên thư mục
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    //Lưu Sạch vào CSDL

                    sach.TenSach = f["sTenSach"];

                    sach.MoTa = f["sMoTa"].Replace("<p>", "").Replace("</p>", "\n");
                    sach.AnhBia = sFileName;

                    sach.NgayCapNhat = Convert.ToDateTime(f["dNgayCapNhat"]); sach.SoLuongBan = int.Parse(f["iSoLuong"]); 
                    sach.GiaBan = decimal.Parse(f["mGiaBan"]); 
                    sach.MaCD =int.Parse(f["MaCD"]); 
                    sach.MaNXB =int.Parse(f["MaNXB"]); 
                    db.SACHes.InsertOnSubmit(sach);
                    db.SubmitChanges();

                    // Về lại trang Quản lý sách
                    return RedirectToAction("BookManagement");

                }
                return View();

            }
        }
        public static string SanitizeHtml(string html)
        {
            // Loại bỏ các thẻ HTML nguy hiểm.
            html = Regex.Replace(html, @"<script[^>]*>.*?</script>|<[^>]+>", "", RegexOptions.Compiled);

            // Chuyển đổi các ký tự đặc biệt thành các thực thể HTML.
            html = HttpUtility.HtmlEncode(html);

            return html;
        }

        private string filter(string mota)
        {
            // Load HTML content into an HtmlDocument
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(mota);

            // Extract plain text from the HTML
            string plainText = doc.DocumentNode.InnerText;

            return plainText;
        }
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            SACH sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);

            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe", sach.MaCD);
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB", sach.MaNXB);

            return View(sach);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(SACH sach, HttpPostedFileBase fileUpload)
        {
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

            if (fileUpload == null)
            {
                ViewBag.Thongbao = "Vui lòng chọn ảnh  bìa";

                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/"), fileName);
                    sach.MoTa = HtmlToText.convert(sach.MoTa);

                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.Thongbao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileUpload.SaveAs(path);
                        sach.AnhBia = fileName;
                        // Query the database for the row to be updated.
                        var query =
                            from ord in db.SACHes
                            where ord.MaSach == sach.MaSach
                            select ord;

                        // Execute the query, and change the column values
                        // you want to change.
                        foreach (SACH book in query)
                        {
                            book.TenSach = sach.TenSach;
                            book.MoTa = sach.MoTa;
                            book.AnhBia = sach.AnhBia;
                            book.NgayCapNhat = sach.NgayCapNhat;
                            book.SoLuongBan = sach.SoLuongBan;
                            book.GiaBan = sach.GiaBan;
                            book.MaCD = sach.MaCD;
                            book.MaNXB = sach.MaNXB;
                            // Insert any additional changes to column values.
                        }

                        // Submit the changes to the database.
                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            // Provide for exceptions.
                        }
                    }
                }
                return RedirectToAction("BookManagement");
            }
        }
        [HttpGet]
        public ActionResult Details(int? id)
        {
            SACH sach = db.SACHes.SingleOrDefault(n => n.MaSach == id);

            ViewBag.MaSach = sach.MaSach;

            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach);
        }
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            return View();
        }
        [HttpGet]
        public ActionResult DeleteBook(int id)
        {
            SACH book = db.SACHes.SingleOrDefault(n => n.MaSach == id);

            ViewBag.MaSach = book.MaSach;

            if (book == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(book);
        }

        [HttpPost]
        public ActionResult ConfirmDeleteBook(int id)
        {
            SACH book = db.SACHes.SingleOrDefault(n => n.MaSach == id);

            ViewBag.MaSach = book.MaSach;

            if (book == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            // Xóa sách nè
            db.SACHes.DeleteOnSubmit(book);
            // Lưu lại thay đổi
            db.SubmitChanges();

            return RedirectToAction("Bookmanagement");
        }
        public ActionResult IndexTopic()
        {
            var topic = db.CHUDEs.ToList();

            return View(topic);
        }

        public ActionResult EditTopic(int? id)
        {
            CHUDE topic = db.CHUDEs.SingleOrDefault(n => n.MaCD == id);

            if (topic == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(topic);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditTopic(FormCollection collection)
        {
            var id = collection["MaCD"];
            var name = collection["TenChuDe"];

            // Query the database for the row to be updated.
            var query =
                from ord in db.CHUDEs
                where ord.MaCD == Int32.Parse(id)
                select ord;

            // Execute the query, and change the column values
            // you want to change.
            foreach (CHUDE ord in query)
            {
                ord.TenChuDe = name;
            }

            // Submit the changes to the database.
            try
            {
                db.SubmitChanges();

                ViewBag.Message = "Chỉnh sửa thành công";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }

            CHUDE topic = db.CHUDEs.SingleOrDefault(n => n.MaCD == Int32.Parse(id));

            return View(topic);
        }

        [HttpGet]
        public ActionResult CreateTopic()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateTopic(FormCollection collection)
        {
            var name = collection["TenChuDe"];

            // Execute the query, and change the column values
            // you want to change.
            // Create a new Order object.
            CHUDE topic = new CHUDE
            {
                TenChuDe = name,
            };

            db.CHUDEs.InsertOnSubmit(topic);

            // Submit the changes to the database.
            try
            {
                db.SubmitChanges();

                ViewBag.Message = "Thêm thành công";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }

            return View();
        }
        //NXB


        public ActionResult IndexNXB()
        {
            var nxb = db.NHAXUATBANs.ToList();

            return View(nxb);
        }

        [HttpGet]
        public ActionResult EditNXB(int? id)
        {
            NHAXUATBAN nxb = db.NHAXUATBANs.SingleOrDefault(n => n.MaNXB == id);

            if (nxb == null)
            {
                Response.StatusCode = 404;
                return null;
            }


            return View(nxb);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditNXB(FormCollection collection)
        {
            var id = collection["MaNXB"];
            var name = collection["TenNXB"];
            var address = collection["DiaChi"];
            var phone = collection["DienThoai"];

            // Query the database for the row to be updated.
            var query =
                from ord in db.NHAXUATBANs
                where ord.MaNXB == Int32.Parse(id)
                select ord;

            // Execute the query, and change the column values
            // you want to change.
            foreach (NHAXUATBAN ord in query)
            {
                ord.TenNXB = name;
                ord.DiaChi = address;
                ord.DienThoai = phone;

            }

            // Submit the changes to the database.
            try
            {
                db.SubmitChanges();

                ViewBag.Message = "Chỉnh sửa thành công";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }

            NHAXUATBAN nxb = db.NHAXUATBANs.SingleOrDefault(n => n.MaNXB == Int32.Parse(id));

            return View(nxb);
        }
        
        public ActionResult News(int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 5;
            var news= db.TRANGTINs.ToList().OrderBy(n => n.MaTT).ToPagedList(pageNumber, pageSize);
            return View(news);
        }
        [HttpGet]
        public ActionResult EditNews(int? id)
        {
            TRANGTIN news = db.TRANGTINs.SingleOrDefault(t => t.MaTT == id);

            if (news == null)
            {
                Response.StatusCode = 404;
                return null;
            }

     
            return View(news);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditNews(TRANGTIN trangtin)
        {

            

            // Query the database for the row to be updated.
            var query =
                from ord in db.TRANGTINs
                where ord.MaTT == trangtin.MaTT
                select ord;

            // Execute the query, and change the column values
            // you want to change.
            foreach (TRANGTIN ord in query)
            {
                ord.TenTrang = trangtin.TenTrang;
                trangtin.NoiDung = HtmlToText.convert(trangtin.NoiDung);
                ord.NoiDung = filter(trangtin.NoiDung);
                ord.NgayTao = DateTime.Now;
                ord.MetaTitle = RemoveDiacriticsAndSpaces(trangtin.TenTrang);
            }

            // Submit the changes to the database.
            try
            {
                db.SubmitChanges();

                ViewBag.Message = "Chỉnh sửa thành công";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }

            TRANGTIN news = db.TRANGTINs.SingleOrDefault(n => n.MaTT == trangtin.MaTT);
            return View(news);
        }
        [HttpGet]
        public ActionResult CreateNews()
        {
           
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateNews(TRANGTIN tt)
        {
            

            tt.NoiDung = HtmlToText.convert(tt.NoiDung);
            if (ModelState.IsValid)
            {
                
                string metaTitle = RemoveDiacriticsAndSpaces(tt.TenTrang);
                tt.NoiDung = filter(tt.NoiDung);
                tt.MetaTitle = metaTitle;
                tt.NgayTao = DateTime.Now;
                db.TRANGTINs.InsertOnSubmit(tt);
                db.SubmitChanges();
                return RedirectToAction("News");
            }
            else
            {
                // ModelState.IsValid là false, do đó hiển thị lại biểu mẫu với thông báo lỗi
                return View();
            }
        }
        public ActionResult DeleteNews(int? id)
        {
            return View();
        }
        [HttpGet]
        public ActionResult DeleteNews(int id)
        {
            
            TRANGTIN trangtin = db.TRANGTINs.SingleOrDefault(n => n.MaTT == id);
            ViewBag.MaTT = trangtin.MaTT;

            if (trangtin == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(trangtin);
        }
        [HttpPost]
        public ActionResult ConfirmDeleteNews(int id)
        {
            TRANGTIN trangtin = db.TRANGTINs.SingleOrDefault(n=>n.MaTT==id);
            ViewBag.MaTT = trangtin.MaTT;

            if (trangtin == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            // Xóa trang tin
            db.TRANGTINs.DeleteOnSubmit(trangtin);
            // Lưu lại thay đổi
            db.SubmitChanges();

            return RedirectToAction("News");
        }
        private string RemoveDiacriticsAndSpaces(string input)
        {
            // Loại bỏ dấu và thay thế dấu cách bằng dấu gạch ngang
            input = new string(input
                .Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return input.Replace(" ", "-");
        }
        public ActionResult Menus()
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(m => m.ParentId == null).OrderBy(m => m.OrderNumber).ToList();
            int[] a = new int[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                var L = db.MENUs.Where(m => m.ParentId == lst[i].id);
                a[i] = L.Count();
            }
            ViewBag.lst = a;
            return View(lst);
        }
        [HttpGet]
        public ActionResult EditMenus(int? id)
        {
            MENU menu = db.MENUs.SingleOrDefault(t => t.id == id);
            var menuList = db.MENUs.ToList();

            // Thêm một tùy chọn cho "menu gốc"
            menuList.Insert(0, new MENU { id = 0, MenuName = "Menu gốc" });

            ViewBag.id = new SelectList(menuList.OrderBy(n => n.id), "id", "MenuName");

            if (menu == null)
            {
                Response.StatusCode = 404;
                return null;
            }


            return View(menu);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditMenus(MENU menu,int id)
        {



            // Query the database for the row to be updated.
            var query =
                from ord in db.MENUs
                where ord.id == menu.id
                select ord;

            // Execute the query, and change the column values
            // you want to change.
            foreach (MENU ord in query)
            {
                ord.MenuName = menu.MenuName;
                if(id ==0)
                {
                    ord.ParentId = null;
                }
                else ord.ParentId = id;
                
                ord.OrderNumber = menu.OrderNumber;
                ord.MenuLink = RemoveDiacriticsAndSpaces(menu.MenuName);
            }

            // Submit the changes to the database.
            try
            {
                db.SubmitChanges();

                ViewBag.Message = "Chỉnh sửa thành công";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // Provide for exceptions.
            }

            MENU mn = db.MENUs.SingleOrDefault(n => n.id == menu.id);
            return View(mn);
        }
        [HttpGet]
        public ActionResult CreateMenus()
        {
            var menuList = db.MENUs.ToList();

            // Thêm một tùy chọn cho "menu gốc"
            menuList.Insert(0, new MENU { id = 0, MenuName = "Menu gốc" });

            ViewBag.id = new SelectList(menuList.OrderBy(n => n.id), "id", "MenuName");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateMenus(MENU menu,int id)
        {
 
            if (ModelState.IsValid)
            {
                if(id ==0)
                {
                    menu.ParentId = null;
                }
                else menu.ParentId = id;
                string link = RemoveDiacriticsAndSpaces(menu.MenuName);
                menu.MenuLink = link;
                db.MENUs.InsertOnSubmit(menu);
                db.SubmitChanges();
                return RedirectToAction("Menus");
            }
            else
            {
                // ModelState.IsValid là false, do đó hiển thị lại biểu mẫu với thông báo lỗi
                return View();
            }
        }

        public ActionResult DeleteMenus(int? id)
        {
            return View();
        }
        [HttpGet]
        public ActionResult DeleteMenus(int id)
        {

            MENU menu = db.MENUs.SingleOrDefault(n => n.id == id);
            ViewBag.MaTT = menu.id;

            if (menu == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(menu);
        }
        [HttpPost]
        public ActionResult ConfirmDeleteMenus(int id)
        {
            MENU menu = db.MENUs.SingleOrDefault(n => n.id == id);
            ViewBag.MaTT = menu.id;
            if (menu == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            // Xóa trang tin
            db.MENUs.DeleteOnSubmit(menu);
            // Lưu lại thay đổi
            db.SubmitChanges();

            return RedirectToAction("Menus");
        }

        [ChildActionOnly]
        public ActionResult ChildMenuPartial(int parentid)
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(m => m.ParentId == parentid).OrderBy(m => m.OrderNumber).ToList();
            ViewBag.Count = lst.Count;
            int[] a = new int[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                var L = db.MENUs.Where(m => m.ParentId == lst[i].id);
                a[i] = L.Count();
            }
            ViewBag.lst = a;
            return PartialView("ChildMenuPartial", lst);
        }

    }
}