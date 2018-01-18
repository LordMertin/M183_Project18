using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiniBlogEngine.Models;

namespace MiniBlogEngine.Controllers
{
    public class UserController : Controller
    {
        Entities db = new Entities();

        public ActionResult Dashboard()
        {
            if (Request.Cookies["authentication_cookie"] != null)
            {
                DashboardModel model = new DashboardModel();
                HttpCookie cookie = new HttpCookie("authentication_cookie");
                cookie = Request.Cookies["authentication_cookie"];
                model.User =
                    db.Users.SingleOrDefault(u => u.Username == cookie.Value);
                //only user-posts and not deleted
                model.Posts = db.Posts.Where(p => p.UserId == model.User.Id && p.DeletedOn == null).ToList();
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}