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
    public class Tac_GiaController : Controller
    {
        private Web_Entities db = new Web_Entities();

        // GET: Admin/Tac_Gia
        public ActionResult Index()
        {
            return View(db.Tac_Gia.ToList());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_TAC_GIA,TEN_TAC_GIA,MO_TA")] Tac_Gia tac_Gia)
        {
            if (ModelState.IsValid)
            {
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

                return RedirectToAction("Index");
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_TAC_GIA,TEN_TAC_GIA,MO_TA")] Tac_Gia tac_Gia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tac_Gia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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
            Tac_Gia tac_Gia = db.Tac_Gia.Find(id);
            db.Tac_Gia.Remove(tac_Gia);
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
