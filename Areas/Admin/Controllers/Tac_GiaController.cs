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
    public class Tac_GiaController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Tac_Gia
        public ActionResult Index(string searchString, string sortOrder, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DescriptionSortParm = sortOrder == "Description" ? "description_desc" : "Description";
            ViewBag.CurrentFilter = searchString;

            var tacGia = from s in db.Tac_Gia
                         select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                tacGia = tacGia.Where(s => s.TEN_TAC_GIA.Contains(searchString) || 
                                          (s.MO_TA != null && s.MO_TA.Contains(searchString)));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tacGia = tacGia.OrderByDescending(s => s.TEN_TAC_GIA);
                    break;
                case "Description":
                    tacGia = tacGia.OrderBy(s => s.MO_TA);
                    break;
                case "description_desc":
                    tacGia = tacGia.OrderByDescending(s => s.MO_TA);
                    break;
                default:
                    tacGia = tacGia.OrderBy(s => s.TEN_TAC_GIA);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(tacGia.ToPagedList(pageNumber, pageSize));
        }

        // GET: Admin/Tac_Gia/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tac_Gia tac_Gia = db.Tac_Gia.Find(id);
            if (tac_Gia == null)
            {
                return HttpNotFound();
            }
            return View(tac_Gia);
        }

        // GET: Admin/Tac_Gia/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Tac_Gia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_TAC_GIA,TEN_TAC_GIA,MO_TA")] Tac_Gia tac_Gia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên tác giả đã tồn tại
                    if (db.Tac_Gia.Any(t => t.TEN_TAC_GIA.ToLower() == tac_Gia.TEN_TAC_GIA.ToLower()))
                    {
                        ModelState.AddModelError("TEN_TAC_GIA", "Tên tác giả đã tồn tại!");
                        return View(tac_Gia);
                    }

                    // Lấy ID_TAC_GIA lớn nhất trong bảng (nếu có)
                    int nextID = 1;
                    if (db.Tac_Gia.Any())
                    {
                        nextID = db.Tac_Gia.Max(t => t.ID_TAC_GIA) + 1;
                    }

                    // Gán ID mới
                    tac_Gia.ID_TAC_GIA = nextID;

                    // Thêm vào DB
                    db.Tac_Gia.Add(tac_Gia);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm tác giả thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(tac_Gia);
        }

        // GET: Admin/Tac_Gia/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tac_Gia tac_Gia = db.Tac_Gia.Find(id);
            if (tac_Gia == null)
            {
                return HttpNotFound();
            }
            return View(tac_Gia);
        }

        // POST: Admin/Tac_Gia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_TAC_GIA,TEN_TAC_GIA,MO_TA")] Tac_Gia tac_Gia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên tác giả đã tồn tại (trừ chính nó)
                    if (db.Tac_Gia.Any(t => t.TEN_TAC_GIA.ToLower() == tac_Gia.TEN_TAC_GIA.ToLower() && 
                                           t.ID_TAC_GIA != tac_Gia.ID_TAC_GIA))
                    {
                        ModelState.AddModelError("TEN_TAC_GIA", "Tên tác giả đã tồn tại!");
                        return View(tac_Gia);
                    }

                    db.Entry(tac_Gia).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Cập nhật tác giả thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            return View(tac_Gia);
        }

        // GET: Admin/Tac_Gia/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tac_Gia tac_Gia = db.Tac_Gia.Find(id);
            if (tac_Gia == null)
            {
                return HttpNotFound();
            }
            return View(tac_Gia);
        }

        // POST: Admin/Tac_Gia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Tac_Gia tac_Gia = db.Tac_Gia.Find(id);
                if (tac_Gia == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra xem tác giả có được sử dụng trong sản phẩm không
                if (db.San_Pham.Any(s => s.ID_TAC_GIA == id))
                {
                    TempData["ErrorMessage"] = "Không thể xóa tác giả này vì đang được sử dụng trong sản phẩm!";
                    return RedirectToAction("Index");
                }

                db.Tac_Gia.Remove(tac_Gia);
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Xóa tác giả thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // AJAX: Kiểm tra tên tác giả đã tồn tại
        [HttpPost]
        public JsonResult CheckAuthorName(string tenTacGia, int? id = null)
        {
            try
            {
                bool exists = false;
                if (id.HasValue)
                {
                    // Kiểm tra khi edit (trừ chính nó)
                    exists = db.Tac_Gia.Any(t => t.TEN_TAC_GIA.ToLower() == tenTacGia.ToLower() && 
                                                t.ID_TAC_GIA != id.Value);
                }
                else
                {
                    // Kiểm tra khi create
                    exists = db.Tac_Gia.Any(t => t.TEN_TAC_GIA.ToLower() == tenTacGia.ToLower());
                }

                return Json(new { exists = exists }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { exists = false, error = "Có lỗi xảy ra" }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Lấy thông tin tác giả
        [HttpGet]
        public JsonResult GetAuthorInfo(int id)
        {
            try
            {
                var tacGia = db.Tac_Gia.Find(id);
                if (tacGia != null)
                {
                    return Json(new
                    {
                        id = tacGia.ID_TAC_GIA,
                        tenTacGia = tacGia.TEN_TAC_GIA,
                        moTa = tacGia.MO_TA
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
