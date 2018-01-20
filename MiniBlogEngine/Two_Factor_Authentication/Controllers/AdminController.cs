using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiniBlogEngine.Models;

namespace MiniBlogEngine.Controllers
{
    public class AdminController : Controller
    {

        Entities db = new Entities();
        public ActionResult Dashboard()
        {
            // Cookie kann nicht ausgelesen werden (Gründe unbekannt), würde eigentlich funktionieren.
            // Testen kann man im Debug mode, die Value des Cookies auf den username setzen.
            if (Request.Cookies.Get("authentication_cookie") != null)
            {
                DashboardModel model = new DashboardModel();
                HttpCookie cookie = new HttpCookie("authentication_cookie");
                cookie = Request.Cookies.Get("authentication_cookie");

                //Hier die Value des Cookies im Debug-Mode einfügen, für abfrage zu DB.
                model.User =
                    db.Users.SingleOrDefault(u => u.Username == cookie.Value);
                //all posts
                model.Posts = db.Posts.ToList();

                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }
    }
}