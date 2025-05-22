using NguyenDuyDuong.SachOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using IBook.Content.module;
using System.IO;
using System.Web.UI.WebControls;

namespace NguyenDuyDuong.SachOnline.Areas.Admin.Controllers
{
    public class SachController : Controller
    {
        private dbDataContext db = new dbDataContext("Data Source=MSI;Initial Catalog=SachOnline;Integrated Security=True");

        // GET: Admin/Sach
        public ActionResult Index(int? page)
        {
            ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");
            int pageNum = (page ?? 1);
            int pageSize = 7;
            return View(db.SACHes.ToList().OrderByDescending(n => n.NgayCapNhat).ToPagedList(pageNum, pageSize));
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(string strTenSach)
        {
            try
            {
                var s = new SACH();
                s.TenSach = strTenSach;
                db.SACHes.InsertOnSubmit(s);
                db.SubmitChanges();
                return Json(new { code = 200, msg = "Thêm sách thành công" },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Thêm sách thất bại" + e.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(SACH sach, HttpPostedFileBase fileUpload)
        {
            try
            {
                ViewBag.MaCD = new SelectList(db.CHUDEs.ToList().OrderBy(n => n.TenChuDe), "MaCD", "TenChuDe");
                ViewBag.MaNXB = new SelectList(db.NHAXUATBANs.ToList().OrderBy(n => n.TenNXB), "MaNXB", "TenNXB");

                if (fileUpload == null)
                {
                    ViewBag.Thongbao = "Vui lòng chọn ảnh bìa";
                    return View();
                }

                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/"), fileName);
                    sach.MoTa = HtmlToText.convert(sach.MoTa);

                    // Query the database for the row to be updated.
                    var book = db.SACHes.FirstOrDefault(b => b.MaSach == sach.MaSach);

                    if (book != null)
                    {
                        if (System.IO.File.Exists(path))
                        {
                            ViewBag.Thongbao = "Hình ảnh đã tồn tại";
                        }
                        else
                        {
                            fileUpload.SaveAs(path);
                            sach.AnhBia = fileName;
                            // Update the properties of the existing book.
                            book.TenSach = sach.TenSach;
                            book.MoTa = sach.MoTa;
                            book.AnhBia = sach.AnhBia;
                            book.SoLuongBan = sach.SoLuongBan;
                            book.GiaBan = sach.GiaBan;
                            book.MaCD = sach.MaCD;
                            book.MaNXB = sach.MaNXB;
                            // You may choose to update or not update NgayCapNhat here.

                            db.SubmitChanges();
                            return Json(new { code = 200, msg = "Sửa sách thành công" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                    
                        return Json(new { code = 500, msg = "Không tìm thấy sách để sửa." }, JsonRequestBehavior.AllowGet);
                    }
                }

                return View();
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Sửa sách thất bại" + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Delete(int maSach)
        {
            try
            {
                var s = db.SACHes.SingleOrDefault(model => model.MaSach == maSach);
                db.SACHes.DeleteOnSubmit(s);
                db.SubmitChanges();
                return Json(new { code = 200, msg = "Xoá sách thành công" },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Xoá sách thất bại" + e.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DsSach()
        {
            try
            {

                var dsSach = (from s in db.SACHes
                              select new
                              {
                                  MaSach = s.MaSach,
                                  MoTa = s.MoTa,
                                  TenSach = s.TenSach,
                                  Anh = s.AnhBia,
                                  NgayCapNhat = s.NgayCapNhat,
                                  SoLuong = s.SoLuongBan,
                                  GiaBan = s.GiaBan,
                                  TenCD = from cd in db.CHUDEs where cd.MaCD == s.MaCD select cd.TenChuDe,
                                  TenNXB = from nxb in db.NHAXUATBANs where nxb.MaNXB == s.MaNXB select nxb.TenNXB
                              }).ToList();
                return Json(new { code = 200, dsSach = dsSach, msg = "Lấy danh sách sách thành công" },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy danh sách sách thất bại" + e.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult TruncateSachDetails(int maSach, int maxTenSachLength, int maxMoTaLength)
        {
            try
            {
                var sach = db.SACHes.SingleOrDefault(model => model.MaSach == maSach);
                string truncatedTenSach = sach.TenSach.Length > maxTenSachLength ? sach.TenSach.Substring(0, maxTenSachLength) + "..." : sach.TenSach;
                string truncatedMoTa = sach.MoTa.Length > maxMoTaLength ? sach.MoTa.Substring(0, maxMoTaLength) + "..." : sach.MoTa;
                return Json(new { code = 200, truncatedTenSach, truncatedMoTa, msg = "Cắt thông tin sách thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Cắt thông tin sách thất bại" + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult Details(int maSach)
        {
            try
            {
                var sach = (from s in db.SACHes
                            where (s.MaSach == maSach)
                            select new
                            {
                                MaSach = s.MaSach,
                                TenSach = s.TenSach,
                                MoTa = s.MoTa,
                                Anh = s.AnhBia,
                                NgayCapNhat = s.NgayCapNhat,
                                SoLuong = s.SoLuongBan,
                                GiaBan = s.GiaBan,
                                TenCD = from cd in db.CHUDEs where cd.MaCD == s.MaCD select cd.TenChuDe,
                                TenNXB = from nxb in db.NHAXUATBANs where nxb.MaNXB == s.MaNXB select nxb.TenNXB
                            }).SingleOrDefault();
                return Json(new { code = 200, sach = sach, msg = "Lấy thông tin sách thành công" },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin sách thất bại" + e.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(int maSach, string tenSach, string moTa)
        {
            try
            {
                var sach = db.SACHes.SingleOrDefault(c => c.MaSach == maSach);
                sach.TenSach = tenSach;
                sach.MoTa = moTa;
                db.SubmitChanges();
                return Json(new { code = 200, msg = "Sửa chủ đề thành công" },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Sửa chủ đề thất bại" + e.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}