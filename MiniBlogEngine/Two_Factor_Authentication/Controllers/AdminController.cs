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
            if (Request.Cookies["authentication_cookie"] != null)
            {
                DashboardModel model = new DashboardModel();
                model.User =
                    db.Users.SingleOrDefault(u => u.Username == Response.Cookies["authentication_cookie"].Value);
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