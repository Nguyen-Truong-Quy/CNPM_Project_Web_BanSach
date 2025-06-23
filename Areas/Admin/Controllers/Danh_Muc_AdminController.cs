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
    public class Danh_Muc_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Danh_Muc_Admin
        public ActionResult Index()
        {
            return View(db.Danh_Muc.ToList());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_DANH_MUC,TEN_DANH_MUC")] Danh_Muc danh_Muc)
        {
            if (ModelState.IsValid)
            {
                db.Danh_Muc.Add(danh_Muc);
                db.SaveChanges();
                return RedirectToAction("Index");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_DANH_MUC,TEN_DANH_MUC")] Danh_Muc danh_Muc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(danh_Muc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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
            db.Danh_Muc.Remove(danh_Muc);
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
