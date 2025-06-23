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
    public class The_Loai_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/The_Loai_Admin
        public ActionResult Index()
        {
            return View(db.The_Loai.ToList());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_The_Loai,Ten_The_Loai")] The_Loai the_Loai)
        {
            if (ModelState.IsValid)
            {
                db.The_Loai.Add(the_Loai);
                db.SaveChanges();
                return RedirectToAction("Index");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_The_Loai,Ten_The_Loai")] The_Loai the_Loai)
        {
            if (ModelState.IsValid)
            {
                db.Entry(the_Loai).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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
            The_Loai the_Loai = db.The_Loai.Find(id);
            db.The_Loai.Remove(the_Loai);
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
