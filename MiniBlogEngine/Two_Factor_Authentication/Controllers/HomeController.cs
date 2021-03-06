﻿using System;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MiniBlogEngine.Models;

namespace MiniBlogEngine.Controllers
{
    public class HomeController : Controller
    {
        MiniBlogEngine.Models.Entities db = new Entities();

        public ActionResult Index()
        {

            // Wenn Session aktiv, dann nicht Startseite sondern Dashboard (Cookie kann aber nicht ausgelesen werden, deshalb auskommentiert.)
            //if (HttpContext.Request.Cookies.Get("authentication_cookie") != null)
            //{
            //    User user = db.Users
            //        .SingleOrDefault(u => u.Username == Request.Cookies["authentication_cookie"].Value);
            //    if (user.Role == "admin")
            //    {
            //        return RedirectToAction("Dashboard", "Admin");
            //    }
            //    else
            //    {
            //        return RedirectToAction("Dashboard", "User");
            //    }
            //}

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

       
        public ActionResult Login()
        {
            var username = Request["username"];
            var password = Request["password"];

            User user = db.Users.SingleOrDefault(u => u.Username == username && u.Password == password);

            //Verify if user exists
            if (user != null && user.Status == "active")
            {
                var request = (HttpWebRequest) WebRequest.Create("https://rest.nexmo.com/sms/json");
                GetRequestWithToken(request, user); 
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }                       
            
            return View(user);
        }

        [HttpPost]
        public ActionResult TokenLogin()
        {
            var tokenreq = Request["token"];
            string username = Request["username"];
            string password = Request["password"];

            //Verify credentials
            User user = db.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            Token token = db.Tokens.Where(t => t.UserId == user.Id).OrderByDescending(t => t.Expiry).First();

            //Verify token
            if (tokenreq == token.Token1 && token.Expiry >= DateTime.Now && token.DeletedOn == null)
            {
                // Add Cookie
                AddCookie(user);

                // Mark Token as deleted
                token.DeletedOn = DateTime.Now;
                db.Tokens.AddOrUpdate(token);
                db.SaveChanges();

                //Verify Role and move to Dashboard
                if (user.Role == "admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Dashboard", "User");
                }
            }
            else
            {
                Log("Login failed", user);
                return RedirectToAction("Login");
            }
            
        }

        [HttpGet]
        public ActionResult Logout()
        {
            if (Request.Cookies.Get("authentication_cookie") != null)
            {
                HttpCookie cookie = Request.Cookies.Get("authentication_cookie");

                // Mark as deleted in DB
                Userlogin login = db.Userlogins.SingleOrDefault(u => u.SessionId == cookie.Name);
                login.DeletedOn = DateTime.Now;
                db.Userlogins.AddOrUpdate(login);
                db.SaveChanges();

                // Delete Cookie
                cookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(cookie);

                User user = db.Users.SingleOrDefault(u => u.Id == login.UserId);
                Log("User logged out", user);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private void AddCookie(User user)
        {
            //Add Cookie
            var authCookie = new HttpCookie("authentication_cookie");
            authCookie.Value = user.Username;
            authCookie.Expires = DateTime.Now.AddDays(14);
            authCookie.Path = "localhost:50446";
            Response.Cookies.Add(authCookie);

            //Add UserLogin-Session
            Userlogin login = new Userlogin();
            login.CreatedOn = DateTime.Now;
            login.UserId = user.Id;
            login.SessionId = authCookie.Name;
            db.Userlogins.Add(login);

            //Log
            Log("Session startet", user);
            db.SaveChanges();
        }

        public void GetRequestWithToken(HttpWebRequest request, User user)
        {
            // Create secret token
            Random rnd = new Random();
            var secret = rnd.Next(1, 50).ToString()
                         + rnd.Next(1, 20).ToString()
                         + rnd.Next(1, 100).ToString()
                         + rnd.Next(1, 10).ToString();

            //Add token to db
            Token token = new Token();
            token.UserId = user.Id;
            token.Token1 = secret;
            token.Expiry = DateTime.Now.AddMinutes(5);
            db.Tokens.Add(token);
            db.SaveChanges();

            //Create SMS message
            var postData = "api_key=243e8477";
            postData += "&api_secret=9602c2376b4ccc20";
            postData += "&to=" + user.Mobilephonenumber;
            postData += "&from=\"\"NEXMO\"\"";
            postData += "&text=\"" + secret + "\"";
            var data = Encoding.ASCII.GetBytes(postData);
            
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            ViewBag.Message = responseString;
        }

        private void Log(string message, User user)
        {
            //Log current happening
            UserLog log = new UserLog();
            log.Action = message;
            log.UserId = user.Id;
            db.UserLogs.Add(log);
            db.SaveChanges();
        }
    }
}