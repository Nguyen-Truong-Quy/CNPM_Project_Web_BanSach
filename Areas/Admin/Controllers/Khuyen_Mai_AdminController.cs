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
    public class Khuyen_Mai_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Khuyen_Mai_Admin
        public ActionResult Index()
        {
            return View(db.Khuyen_Mai.ToList());
        }

        // GET: Admin/Khuyen_Mai_Admin/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Khuyen_Mai khuyen_Mai = db.Khuyen_Mai.Find(id);
            if (khuyen_Mai == null)
            {
                return HttpNotFound();
            }
            return View(khuyen_Mai);
        }

        // GET: Admin/Khuyen_Mai_Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Khuyen_Mai_Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MA_KHUYEN_MAI,TEN_KHUYEN_MAI,PHAN_TRAM_GIAM,NGAY_BAT_DAU,NGAY_KET_THUC")] Khuyen_Mai khuyen_Mai)
        {
            if (ModelState.IsValid)
            {
                db.Khuyen_Mai.Add(khuyen_Mai);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(khuyen_Mai);
        }

        // GET: Admin/Khuyen_Mai_Admin/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Khuyen_Mai khuyen_Mai = db.Khuyen_Mai.Find(id);
            if (khuyen_Mai == null)
            {
                return HttpNotFound();
            }
            return View(khuyen_Mai);
        }

        // POST: Admin/Khuyen_Mai_Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MA_KHUYEN_MAI,TEN_KHUYEN_MAI,PHAN_TRAM_GIAM,NGAY_BAT_DAU,NGAY_KET_THUC")] Khuyen_Mai khuyen_Mai)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khuyen_Mai).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(khuyen_Mai);
        }

        // GET: Admin/Khuyen_Mai_Admin/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Khuyen_Mai khuyen_Mai = db.Khuyen_Mai.Find(id);
            if (khuyen_Mai == null)
            {
                return HttpNotFound();
            }
            return View(khuyen_Mai);
        }

        // POST: Admin/Khuyen_Mai_Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Khuyen_Mai khuyen_Mai = db.Khuyen_Mai.Find(id);
            db.Khuyen_Mai.Remove(khuyen_Mai);
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
