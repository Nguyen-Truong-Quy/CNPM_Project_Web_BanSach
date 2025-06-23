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
    public class Nha_Xuat_Ban_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Nha_Xuat_Ban_Admin
        public ActionResult Index()
        {
            return View(db.Nha_Xuat_Ban.ToList());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_NXB,TEN_NXB,DIA_CHI,EMAIL")] Nha_Xuat_Ban nha_Xuat_Ban)
        {
            if (ModelState.IsValid)
            {
                db.Nha_Xuat_Ban.Add(nha_Xuat_Ban);
                db.SaveChanges();
                return RedirectToAction("Index");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_NXB,TEN_NXB,DIA_CHI,EMAIL")] Nha_Xuat_Ban nha_Xuat_Ban)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nha_Xuat_Ban).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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
            Nha_Xuat_Ban nha_Xuat_Ban = db.Nha_Xuat_Ban.Find(id);
            db.Nha_Xuat_Ban.Remove(nha_Xuat_Ban);
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
