using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;
using PagedList;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class Nha_Xuat_Ban_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Nha_Xuat_Ban_Admin
        public ActionResult Index(string searchString, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.AddressSortParm = sortOrder == "Address" ? "address_desc" : "Address";
            ViewBag.EmailSortParm = sortOrder == "Email" ? "email_desc" : "Email";
            ViewBag.CurrentFilter = searchString;

            var nhaXuatBan = from s in db.Nha_Xuat_Ban
                              select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                nhaXuatBan = nhaXuatBan.Where(s => s.TEN_NXB.Contains(searchString) || 
                                                   (s.DIA_CHI != null && s.DIA_CHI.Contains(searchString)) ||
                                                   (s.EMAIL != null && s.EMAIL.Contains(searchString)));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    nhaXuatBan = nhaXuatBan.OrderByDescending(s => s.TEN_NXB);
                    break;
                case "Address":
                    nhaXuatBan = nhaXuatBan.OrderBy(s => s.DIA_CHI);
                    break;
                case "address_desc":
                    nhaXuatBan = nhaXuatBan.OrderByDescending(s => s.DIA_CHI);
                    break;
                case "Email":
                    nhaXuatBan = nhaXuatBan.OrderBy(s => s.EMAIL);
                    break;
                case "email_desc":
                    nhaXuatBan = nhaXuatBan.OrderByDescending(s => s.EMAIL);
                    break;
                default:
                    nhaXuatBan = nhaXuatBan.OrderBy(s => s.TEN_NXB);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(nhaXuatBan.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Nha_Xuat_Ban_Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nha_Xuat_Ban nha_Xuat_Ban = db.Nha_Xuat_Ban.Find(id);
            if (nha_Xuat_Ban == null)
            {
                return HttpNotFound();
            }
            return View(nha_Xuat_Ban);
        }

        // GET: Admin/Nha_Xuat_Ban_Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Nha_Xuat_Ban_Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_NXB,TEN_NXB,DIA_CHI,EMAIL")] Nha_Xuat_Ban nha_Xuat_Ban)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên nhà xuất bản đã tồn tại
                    if (db.Nha_Xuat_Ban.Any(n => n.TEN_NXB.ToLower() == nha_Xuat_Ban.TEN_NXB.ToLower()))
                    {
                        ModelState.AddModelError("TEN_NXB", "Tên nhà xuất bản đã tồn tại!");
                        return View(nha_Xuat_Ban);
                    }

                    // Lấy ID_NXB lớn nhất trong bảng (nếu có)
                    int nextID = 1;
                    if (db.Nha_Xuat_Ban.Any())
                    {
                        nextID = db.Nha_Xuat_Ban.Max(n => n.ID_NXB) + 1;
                    }

                    // Gán ID mới
                    nha_Xuat_Ban.ID_NXB = nextID;

                    db.Nha_Xuat_Ban.Add(nha_Xuat_Ban);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm nhà xuất bản thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(nha_Xuat_Ban);
        }

        // GET: Admin/Nha_Xuat_Ban_Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nha_Xuat_Ban nha_Xuat_Ban = db.Nha_Xuat_Ban.Find(id);
            if (nha_Xuat_Ban == null)
            {
                return HttpNotFound();
            }
            return View(nha_Xuat_Ban);
        }

        // POST: Admin/Nha_Xuat_Ban_Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_NXB,TEN_NXB,DIA_CHI,EMAIL")] Nha_Xuat_Ban nha_Xuat_Ban)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên nhà xuất bản đã tồn tại (trừ chính nó)
                    if (db.Nha_Xuat_Ban.Any(n => n.TEN_NXB.ToLower() == nha_Xuat_Ban.TEN_NXB.ToLower() && 
                                                 n.ID_NXB != nha_Xuat_Ban.ID_NXB))
                    {
                        ModelState.AddModelError("TEN_NXB", "Tên nhà xuất bản đã tồn tại!");
                        return View(nha_Xuat_Ban);
                    }

                    db.Entry(nha_Xuat_Ban).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Cập nhật nhà xuất bản thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            return View(nha_Xuat_Ban);
        }

        // GET: Admin/Nha_Xuat_Ban_Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nha_Xuat_Ban nha_Xuat_Ban = db.Nha_Xuat_Ban.Find(id);
            if (nha_Xuat_Ban == null)
            {
                return HttpNotFound();
            }
            return View(nha_Xuat_Ban);
        }

        // POST: Admin/Nha_Xuat_Ban_Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Nha_Xuat_Ban nha_Xuat_Ban = db.Nha_Xuat_Ban.Find(id);
                if (nha_Xuat_Ban == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra xem nhà xuất bản có được sử dụng trong sản phẩm không
                if (db.San_Pham.Any(s => s.ID_NXB == id))
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhà xuất bản này vì đang được sử dụng trong sản phẩm!";
                    return RedirectToAction("Index");
                }

                db.Nha_Xuat_Ban.Remove(nha_Xuat_Ban);
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Xóa nhà xuất bản thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // AJAX: Kiểm tra tên nhà xuất bản đã tồn tại
        [HttpPost]
        public JsonResult CheckPublisherName(string tenNXB, int? id = null)
        {
            try
            {
                bool exists = false;
                if (id.HasValue)
                {
                    // Kiểm tra khi edit (trừ chính nó)
                    exists = db.Nha_Xuat_Ban.Any(n => n.TEN_NXB.ToLower() == tenNXB.ToLower() && 
                                                     n.ID_NXB != id.Value);
                }
                else
                {
                    // Kiểm tra khi create
                    exists = db.Nha_Xuat_Ban.Any(n => n.TEN_NXB.ToLower() == tenNXB.ToLower());
                }

                return Json(new { exists = exists }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { exists = false, error = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Lấy thông tin nhà xuất bản
        [HttpGet]
        public JsonResult GetPublisherInfo(int id)
        {
            try
            {
                var nhaXuatBan = db.Nha_Xuat_Ban.Find(id);
                if (nhaXuatBan != null)
                {
                    return Json(new
                    {
                        id = nhaXuatBan.ID_NXB,
                        tenNXB = nhaXuatBan.TEN_NXB,
                        diaChi = nhaXuatBan.DIA_CHI,
                        email = nhaXuatBan.EMAIL
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
