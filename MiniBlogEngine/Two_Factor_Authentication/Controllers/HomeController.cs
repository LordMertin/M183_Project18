using System;
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
            if (Request.Cookies["authentication_cookie"] != null)
            {
                User user = db.Users
                    .SingleOrDefault(u => u.Username == Request.Cookies["authentication_cookie"].Value);
                if (user.Role == "admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Dashboard", "User");
                }
            }

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
            if (user != null && user.Status != "active")
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
            User user = db.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            Token token = db.Tokens.Where(t => t.UserId == user.Id).OrderByDescending(t => t.Expiry).First();

            if (tokenreq == token.Token1 && token.Expiry >= DateTime.Now && token.DeletedOn == null)
            {
                AddCookie(user);
                token.DeletedOn = DateTime.Now;
                db.Tokens.AddOrUpdate(token);
                db.SaveChanges();

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

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private void AddCookie(User user)
        {
            var authCookie = new HttpCookie("authentication_cookie");
            authCookie.Value = user.Username;
            authCookie.Expires = DateTime.Now.AddDays(14);
            authCookie.Path = "localhost:50446";
            Response.Cookies.Add(authCookie);

            Userlogin login = new Userlogin();
            login.CreatedOn = DateTime.Now;
            login.UserId = user.Id;
            login.SessionId = authCookie.Name;
            db.Userlogins.Add(login);

            Log("Session startet", user);
            db.SaveChanges();
        }

        public void GetRequestWithToken(HttpWebRequest request, User user)
        {
            Random rnd = new Random();
            var secret = rnd.Next(1, 50).ToString()
                         + rnd.Next(1, 20).ToString()
                         + rnd.Next(1, 100).ToString()
                         + rnd.Next(1, 10).ToString();

            Token token = new Token();
            token.UserId = user.Id;
            token.Token1 = secret;
            token.Expiry = DateTime.Now.AddMinutes(5);
            db.Tokens.Add(token);
            db.SaveChanges();

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
            UserLog log = new UserLog();
            log.Action = message;
            log.UserId = user.Id;
            db.UserLogs.Add(log);
            db.SaveChanges();
        }
    }
}