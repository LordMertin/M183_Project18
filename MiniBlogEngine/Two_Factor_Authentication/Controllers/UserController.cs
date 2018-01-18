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
            if (Response.Cookies["authentication_cookie"] != null)
            {
                DashboardModel model = new DashboardModel();
                model.User =
                    db.Users.SingleOrDefault(u => u.Username == Response.Cookies["authentication_cookie"].Value);
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