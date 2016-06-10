using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CMS.Models;
using Microsoft.Owin.Security.OAuth.Messages;

namespace CMS.Controllers
{
    public class UserAccountController : Controller
    {
        private CourseManagementDbContext db = new CourseManagementDbContext();


        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(UserAccount user)
        {
            var aUser = db.UserAccounts.Where(x => x.Email == user.Email && x.Password == user.Password);
            foreach (var boss in aUser)
            {
                Session["user_id"] = boss.Id;
                Session["user_email"] = boss.Email;
            }
            if (Session["user_email"] != null)
            {
                return RedirectToAction("Create", "Teacher");
            }
            ViewBag.message = "Email or Password is incorrect!!!";
            return View(user);    
        }

        //// GET: /UserAccount/Create
        public ActionResult Registrar()
        {
            return View();
        }

        // POST: /UserAccount/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar([Bind(Include = "Id,Email,Password")] UserAccount useraccount)
        {
            if (ModelState.IsValid)
            {
                if (!db.UserAccounts.Any(x => x.Email == useraccount.Email))
                {
                    db.UserAccounts.Add(useraccount);
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
            }
            ViewBag.message = "This email is already used. Please register with another email.";
            return View(useraccount);
        }



        //// GET: /UserAccount/
        //public ActionResult Index()
        //{
        //    return View(db.UserAccounts.ToList());
        //}

        //// GET: /UserAccount/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    UserAccount useraccount = db.UserAccounts.Find(id);
        //    if (useraccount == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(useraccount);
        //}

        //// GET: /UserAccount/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: /UserAccount/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include="Id,Email,Password")] UserAccount useraccount)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.UserAccounts.Add(useraccount);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(useraccount);
        //}

        //// GET: /UserAccount/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    UserAccount useraccount = db.UserAccounts.Find(id);
        //    if (useraccount == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(useraccount);
        //}

        //// POST: /UserAccount/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include="Id,Email,Password")] UserAccount useraccount)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(useraccount).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(useraccount);
        //}

        //// GET: /UserAccount/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    UserAccount useraccount = db.UserAccounts.Find(id);
        //    if (useraccount == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(useraccount);
        //}

        //// POST: /UserAccount/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    UserAccount useraccount = db.UserAccounts.Find(id);
        //    db.UserAccounts.Remove(useraccount);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
