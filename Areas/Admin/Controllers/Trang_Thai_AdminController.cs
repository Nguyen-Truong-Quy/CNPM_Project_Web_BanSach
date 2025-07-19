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
    public class Trang_Thai_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Trang_Thai_Admin
        public ActionResult Index(string searchString, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentFilter = searchString;

            var trangThai = from s in db.Trang_Thai
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                trangThai = trangThai.Where(s => s.TEN_TRANG_THAI.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    trangThai = trangThai.OrderByDescending(s => s.TEN_TRANG_THAI);
                    break;
                default:
                    trangThai = trangThai.OrderBy(s => s.TEN_TRANG_THAI);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(trangThai.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Trang_Thai_Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
            if (trang_Thai == null)
            {
                return HttpNotFound();
            }
            return View(trang_Thai);
        }

        // GET: Admin/Trang_Thai_Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Trang_Thai_Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_TRANG_THAI,TEN_TRANG_THAI")] Trang_Thai trang_Thai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên trạng thái đã tồn tại
                    if (db.Trang_Thai.Any(t => t.TEN_TRANG_THAI.ToLower() == trang_Thai.TEN_TRANG_THAI.ToLower()))
                    {
                        ModelState.AddModelError("TEN_TRANG_THAI", "Tên trạng thái đã tồn tại!");
                        return View(trang_Thai);
                    }

                    // Lấy ID_TRANG_THAI lớn nhất trong bảng (nếu có)
                    int nextID = 1;
                    if (db.Trang_Thai.Any())
                    {
                        nextID = db.Trang_Thai.Max(t => t.ID_TRANG_THAI) + 1;
                    }

                    // Gán ID mới
                    trang_Thai.ID_TRANG_THAI = nextID;

                    db.Trang_Thai.Add(trang_Thai);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm trạng thái thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(trang_Thai);
        }

        // GET: Admin/Trang_Thai_Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
            if (trang_Thai == null)
            {
                return HttpNotFound();
            }
            return View(trang_Thai);
        }

        // POST: Admin/Trang_Thai_Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_TRANG_THAI,TEN_TRANG_THAI")] Trang_Thai trang_Thai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên trạng thái đã tồn tại (trừ chính nó)
                    if (db.Trang_Thai.Any(t => t.TEN_TRANG_THAI.ToLower() == trang_Thai.TEN_TRANG_THAI.ToLower() && 
                                               t.ID_TRANG_THAI != trang_Thai.ID_TRANG_THAI))
                    {
                        ModelState.AddModelError("TEN_TRANG_THAI", "Tên trạng thái đã tồn tại!");
                        return View(trang_Thai);
                    }

                    db.Entry(trang_Thai).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            return View(trang_Thai);
        }

        // GET: Admin/Trang_Thai_Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
            if (trang_Thai == null)
            {
                return HttpNotFound();
            }
            return View(trang_Thai);
        }

        // POST: Admin/Trang_Thai_Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Trang_Thai trang_Thai = db.Trang_Thai.Find(id);
                if (trang_Thai == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra xem trạng thái có được sử dụng trong đơn hàng không
                if (db.Don_Hang.Any(d => d.ID_TRANG_THAI == id))
                {
                    TempData["ErrorMessage"] = "Không thể xóa trạng thái này vì đang được sử dụng trong đơn hàng!";
                    return RedirectToAction("Index");
                }

                db.Trang_Thai.Remove(trang_Thai);
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Xóa trạng thái thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // AJAX: Kiểm tra tên trạng thái đã tồn tại
        [HttpPost]
        public JsonResult CheckStatusName(string tenTrangThai, int? id = null)
        {
            try
            {
                bool exists = false;
                if (id.HasValue)
                {
                    // Kiểm tra khi edit (trừ chính nó)
                    exists = db.Trang_Thai.Any(t => t.TEN_TRANG_THAI.ToLower() == tenTrangThai.ToLower() && 
                                                   t.ID_TRANG_THAI != id.Value);
                }
                else
                {
                    // Kiểm tra khi create
                    exists = db.Trang_Thai.Any(t => t.TEN_TRANG_THAI.ToLower() == tenTrangThai.ToLower());
                }

                return Json(new { exists = exists }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { exists = false, error = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Lấy thông tin trạng thái
        [HttpGet]
        public JsonResult GetStatusInfo(int id)
        {
            try
            {
                var trangThai = db.Trang_Thai.Find(id);
                if (trangThai != null)
                {
                    return Json(new
                    {
                        id = trangThai.ID_TRANG_THAI,
                        tenTrangThai = trangThai.TEN_TRANG_THAI
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
