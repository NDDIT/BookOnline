using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using IBook.Content.module;
using NguyenDuyDuong.SachOnline.Models;
using PagedList;
using PagedList.Mvc;
using HtmlAgilityPack;
namespace NguyenDuyDuong.SachOnline.Controllers
{
    public class BookStoreController : Controller
    {
        // GET: BookStore
        dbDataContext db = new dbDataContext("Data Source=MSI;Initial Catalog=SachOnline;Integrated Security=True");
        private List<SACH> LaySachMoi()
        {
            return db.SACHes.OrderByDescending(a => a.NgayCapNhat).ToList();
        }
        private List<SACH> LaySachBanNhieu(int count)
        {
            return db.SACHes.OrderByDescending(a => a.SoLuongBan).Take(count).ToList();
        }

        public ActionResult Index()
        {
            
            return View();
        }
        [ChildActionOnly]
        public ActionResult NavPartial()
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
            return PartialView(lst);
        }
        [ChildActionOnly]
        public ActionResult LoadChildMenuPartial(int parentid)
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
            return PartialView("LoadChildMenuPartial", lst);
        }
        public ActionResult ChuDePartial() 
        {
            var kq= from tt in db.CHUDEs select tt;
            return PartialView(kq);
        }
        public ActionResult NhaXuatBanPartial()
        {
            var kq = from tt in db.NHAXUATBANs select tt;
            return PartialView(kq);
        }
        public ActionResult SlidePartial()
        {
            var model = db.SLIDEs.Where(m => m.HienThi == true).ToList();
            return PartialView(model);
        }
        public ActionResult SachBanNhieuPartial()
        {
            var sachbannhieu = LaySachBanNhieu(6);
            return PartialView(sachbannhieu);
        }
        public ActionResult SachMoiPartial(int ? page)
        {
            int pageSize = 6;
            int pageNum = (page ?? 1);
            var sachmoi = LaySachMoi();
            return PartialView(sachmoi.ToPagedList(pageNum, pageSize));

           // return PartialView(sachmoi);
        }
        public ActionResult SPTheoChuDe(int id, int ? page)
        {
            var sach = from s in db.SACHes where s.MaCD == id select s;
            int pageSize = 6;
            int pageNum = (page ?? 1);
            return PartialView(sach.ToPagedList(pageNum, pageSize));
        }
        public ActionResult SPTheoNhaXuatBan(int id, int? page)
        {
            var sach = from tt  in db.SACHes where tt.MaNXB == id select tt;
            int pageSize = 6;
            int pageNum = (page ?? 1);
            return PartialView(sach.ToPagedList(pageNum, pageSize));
        }
        public ActionResult Details(int id)
        {
            var sach = from s in db.SACHes where s.MaSach == id select s;
            return View(sach.Single());
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
        public ActionResult Create(SACH sach, HttpPostedFileBase fileUpload)
        {
            sach.MoTa = HtmlToText.convert(sach.MoTa);

            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

            if (fileUpload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";

                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    // Lọc lại dữ liệu, nếu không có dòng này trình duyệt sẽ chặn do .. cross-site scripting attack 
                    sach.MoTa = filter(sach.MoTa);

                    var fileName = Path.GetFileName(fileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);

                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.ThongBao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileUpload.SaveAs(path);
                    }
                    sach.AnhBia = fileName;

                    db.SACHes.InsertOnSubmit(sach);
                    db.SubmitChanges();
                }
                return RedirectToAction("BookManagement");
            }
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
        public ActionResult TrangTin(string metatitle)
        {
            var tt = (from t in db.TRANGTINs where t.MetaTitle == metatitle select t).Single();
            return View (tt);
        }
    }
}