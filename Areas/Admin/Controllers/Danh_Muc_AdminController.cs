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
    public class Danh_Muc_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Danh_Muc_Admin
        public ActionResult Index(string searchString, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            var categories = db.Danh_Muc.AsQueryable();

            // Tìm kiếm
            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => 
                    c.TEN_DANH_MUC.Contains(searchString) ||
                    c.ID_DANH_MUC.ToString().Contains(searchString)
                );
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    categories = categories.OrderByDescending(c => c.TEN_DANH_MUC);
                    break;
                default:
                    categories = categories.OrderBy(c => c.TEN_DANH_MUC);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(categories.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Danh_Muc_Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Danh_Muc danh_Muc = db.Danh_Muc.Find(id);
            if (danh_Muc == null)
            {
                return HttpNotFound();
            }
            return View(danh_Muc);
        }

        // GET: Admin/Danh_Muc_Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Danh_Muc_Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_DANH_MUC,TEN_DANH_MUC")] Danh_Muc danh_Muc)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Danh_Muc.Add(danh_Muc);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Danh mục đã được tạo thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi tạo danh mục: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra các trường bắt buộc.";
            }

            return View(danh_Muc);
        }

        // GET: Admin/Danh_Muc_Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Danh_Muc danh_Muc = db.Danh_Muc.Find(id);
            if (danh_Muc == null)
            {
                return HttpNotFound();
            }
            return View(danh_Muc);
        }

        // POST: Admin/Danh_Muc_Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_DANH_MUC,TEN_DANH_MUC")] Danh_Muc danh_Muc)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(danh_Muc).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Danh mục đã được cập nhật thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật danh mục: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra các trường bắt buộc.";
            }

            return View(danh_Muc);
        }

        // GET: Admin/Danh_Muc_Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Danh_Muc danh_Muc = db.Danh_Muc.Find(id);
            if (danh_Muc == null)
            {
                return HttpNotFound();
            }
            return View(danh_Muc);
        }

        // POST: Admin/Danh_Muc_Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Danh_Muc danh_Muc = db.Danh_Muc.Find(id);
            if (danh_Muc == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra ràng buộc trước khi xóa
            var hasProducts = db.San_Pham.Any(p => p.ID_DANH_MUC == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì đã có sản phẩm liên quan!";
                return RedirectToAction("Index");
            }

            try
            {
                db.Danh_Muc.Remove(danh_Muc);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Danh mục đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa danh mục: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // AJAX: Kiểm tra tên danh mục trùng lặp
        [HttpPost]
        public JsonResult CheckCategoryName(string tenDanhMuc, int? id = null)
        {
            var exists = db.Danh_Muc.Any(c => 
                c.TEN_DANH_MUC == tenDanhMuc && 
                (id == null || c.ID_DANH_MUC != id));
            
            return Json(new { exists = exists });
        }

        // AJAX: Lấy thông tin danh mục
        [HttpGet]
        public JsonResult GetCategoryInfo(int id)
        {
            var category = db.Danh_Muc.Find(id);

            if (category != null)
            {
                return Json(new
                {
                    id = category.ID_DANH_MUC,
                    tenDanhMuc = category.TEN_DANH_MUC
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
