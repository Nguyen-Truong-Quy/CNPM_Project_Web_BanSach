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
    public class The_Loai_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/The_Loai_Admin
        public ActionResult Index(string searchString, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentFilter = searchString;

            var theLoai = from s in db.The_Loai
                          select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                theLoai = theLoai.Where(s => s.Ten_The_Loai.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    theLoai = theLoai.OrderByDescending(s => s.Ten_The_Loai);
                    break;
                default:
                    theLoai = theLoai.OrderBy(s => s.Ten_The_Loai);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(theLoai.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/The_Loai_Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            The_Loai the_Loai = db.The_Loai.Find(id);
            if (the_Loai == null)
            {
                return HttpNotFound();
            }
            return View(the_Loai);
        }

        // GET: Admin/The_Loai_Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/The_Loai_Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_The_Loai,Ten_The_Loai")] The_Loai the_Loai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên thể loại đã tồn tại
                    if (db.The_Loai.Any(t => t.Ten_The_Loai.ToLower() == the_Loai.Ten_The_Loai.ToLower()))
                    {
                        ModelState.AddModelError("Ten_The_Loai", "Tên thể loại đã tồn tại!");
                        return View(the_Loai);
                    }

                    // Lấy ID_The_Loai lớn nhất trong bảng (nếu có)
                    int nextID = 1;
                    if (db.The_Loai.Any())
                    {
                        nextID = db.The_Loai.Max(t => t.ID_The_Loai) + 1;
                    }

                    // Gán ID mới
                    the_Loai.ID_The_Loai = nextID;

                    db.The_Loai.Add(the_Loai);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm thể loại thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(the_Loai);
        }

        // GET: Admin/The_Loai_Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            The_Loai the_Loai = db.The_Loai.Find(id);
            if (the_Loai == null)
            {
                return HttpNotFound();
            }
            return View(the_Loai);
        }

        // POST: Admin/The_Loai_Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_The_Loai,Ten_The_Loai")] The_Loai the_Loai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên thể loại đã tồn tại (trừ chính nó)
                    if (db.The_Loai.Any(t => t.Ten_The_Loai.ToLower() == the_Loai.Ten_The_Loai.ToLower() && 
                                             t.ID_The_Loai != the_Loai.ID_The_Loai))
                    {
                        ModelState.AddModelError("Ten_The_Loai", "Tên thể loại đã tồn tại!");
                        return View(the_Loai);
                    }

                    db.Entry(the_Loai).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Cập nhật thể loại thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            return View(the_Loai);
        }

        // GET: Admin/The_Loai_Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            The_Loai the_Loai = db.The_Loai.Find(id);
            if (the_Loai == null)
            {
                return HttpNotFound();
            }
            return View(the_Loai);
        }

        // POST: Admin/The_Loai_Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                The_Loai the_Loai = db.The_Loai.Find(id);
                if (the_Loai == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra xem thể loại có được sử dụng trong sản phẩm không
                if (db.San_Pham.Any(s => s.ID_The_Loai == id))
                {
                    TempData["ErrorMessage"] = "Không thể xóa thể loại này vì đang được sử dụng trong sản phẩm!";
                    return RedirectToAction("Index");
                }

                db.The_Loai.Remove(the_Loai);
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Xóa thể loại thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // AJAX: Kiểm tra tên thể loại đã tồn tại
        [HttpPost]
        public JsonResult CheckCategoryName(string tenTheLoai, int? id = null)
        {
            try
            {
                bool exists = false;
                if (id.HasValue)
                {
                    // Kiểm tra khi edit (trừ chính nó)
                    exists = db.The_Loai.Any(t => t.Ten_The_Loai.ToLower() == tenTheLoai.ToLower() && 
                                                 t.ID_The_Loai != id.Value);
                }
                else
                {
                    // Kiểm tra khi create
                    exists = db.The_Loai.Any(t => t.Ten_The_Loai.ToLower() == tenTheLoai.ToLower());
                }

                return Json(new { exists = exists }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { exists = false, error = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Lấy thông tin thể loại
        [HttpGet]
        public JsonResult GetCategoryInfo(int id)
        {
            try
            {
                var theLoai = db.The_Loai.Find(id);
                if (theLoai != null)
                {
                    return Json(new
                    {
                        id = theLoai.ID_The_Loai,
                        tenTheLoai = theLoai.Ten_The_Loai
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
