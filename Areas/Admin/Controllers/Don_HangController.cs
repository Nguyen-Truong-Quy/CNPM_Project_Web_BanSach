using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CNPM_Project_web.Model;

namespace CNPM_Project_web.Areas.Admin.Controllers
{
    public class Don_HangController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Don_Hang
        public ActionResult Index()
        {
            var don_Hang = db.Don_Hang.Include(d => d.Trang_Thai).Include(d => d.Khach_Hang);
            return View(don_Hang.ToList());
        }

        // GET: Admin/Don_Hang/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            if (don_Hang == null)
            {
                return HttpNotFound();
            }
            return View(don_Hang);
        }

        // GET: Admin/Don_Hang/Create
        public ActionResult Create()
        {
            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI");
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH");
            return View();
        }

        // POST: Admin/Don_Hang/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_DON_HANG,MA_KH,TG_DAT_HANG,ID_TRANG_THAI,TONG_TIEN")] Don_Hang don_Hang)
        {
            if (ModelState.IsValid)
            {
                db.Don_Hang.Add(don_Hang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI", don_Hang.ID_TRANG_THAI);
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", don_Hang.MA_KH);
            return View(don_Hang);
        }

        // GET: Admin/Don_Hang/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            if (don_Hang == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI", don_Hang.ID_TRANG_THAI);
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", don_Hang.MA_KH);
            return View(don_Hang);
        }

        // POST: Admin/Don_Hang/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_DON_HANG,MA_KH,TG_DAT_HANG,ID_TRANG_THAI,TONG_TIEN")] Don_Hang don_Hang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(don_Hang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_TRANG_THAI = new SelectList(db.Trang_Thai, "ID_TRANG_THAI", "TEN_TRANG_THAI", don_Hang.ID_TRANG_THAI);
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", don_Hang.MA_KH);
            return View(don_Hang);
        }

        // GET: Admin/Don_Hang/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            if (don_Hang == null)
            {
                return HttpNotFound();
            }
            return View(don_Hang);
        }

        // POST: Admin/Don_Hang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Don_Hang don_Hang = db.Don_Hang.Find(id);
            db.Don_Hang.Remove(don_Hang);
            db.SaveChanges();
            return RedirectToAction("Index");
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
