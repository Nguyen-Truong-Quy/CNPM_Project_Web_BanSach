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
    public class USERs_AdminController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/USERs_Admin
        public ActionResult Index()
        {
            var uSERS = db.USERS.Include(u => u.Khach_Hang).Include(u => u.Role);
            return View(uSERS.ToList());
        }

        // GET: Admin/USERs_Admin/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER uSER = db.USERS.Find(id);
            if (uSER == null)
            {
                return HttpNotFound();
            }
            return View(uSER);
        }

        // GET: Admin/USERs_Admin/Create
        public ActionResult Create()
        {
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH");
            ViewBag.ID_ROLE = new SelectList(db.Roles, "ID_ROLE", "TEN_ROLE");
            return View();
        }

        // POST: Admin/USERs_Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "USERNAME,PASSWORD,ID_ROLE,MA_KH,EMAIL")] USER uSER)
        {
            if (ModelState.IsValid)
            {
                db.USERS.Add(uSER);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", uSER.MA_KH);
            ViewBag.ID_ROLE = new SelectList(db.Roles, "ID_ROLE", "TEN_ROLE", uSER.ID_ROLE);
            return View(uSER);
        }

        // GET: Admin/USERs_Admin/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER uSER = db.USERS.Find(id);
            if (uSER == null)
            {
                return HttpNotFound();
            }
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", uSER.MA_KH);
            ViewBag.ID_ROLE = new SelectList(db.Roles, "ID_ROLE", "TEN_ROLE", uSER.ID_ROLE);
            return View(uSER);
        }

        // POST: Admin/USERs_Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "USERNAME,PASSWORD,ID_ROLE,MA_KH,EMAIL")] USER uSER)
        {
            if (ModelState.IsValid)
            {
                db.Entry(uSER).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MA_KH = new SelectList(db.Khach_Hang, "MA_KH", "HO_TEN_KH", uSER.MA_KH);
            ViewBag.ID_ROLE = new SelectList(db.Roles, "ID_ROLE", "TEN_ROLE", uSER.ID_ROLE);
            return View(uSER);
        }

        // GET: Admin/USERs_Admin/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            USER uSER = db.USERS.Find(id);
            if (uSER == null)
            {
                return HttpNotFound();
            }
            return View(uSER);
        }

        // POST: Admin/USERs_Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            USER uSER = db.USERS.Find(id);
            db.USERS.Remove(uSER);
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
