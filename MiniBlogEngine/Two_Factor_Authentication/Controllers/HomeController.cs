using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using MiniBlogEngine.Models;

namespace MiniBlogEngine.Controllers
{
    public class HomeController : Controller
    {
        MiniBlogEngine.Models.Entities db = new Entities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

       
        public ActionResult Login()
        {
            var users = db.Users;

            var username = Request["username"];
            var password = Request["password"];

            if (users.Any(u => u.Username == username && u.Password == password))
            {
                var request = (HttpWebRequest) WebRequest.Create("https://rest.nexmo.com/sms/json");
                getRequestWithToken(request); 
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }                       
            
            return View();
        }

        [HttpPost]
        public ActionResult TokenLogin()
        {
            var tokenreq = Request["token"];
            string token = Session["gentoken"] as string;

            if (tokenreq == token)
            {
                ViewBag.Title = "Login successful";
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (tokenreq == token)
            {
                ViewBag.Title = "Login successful";
                return RedirectToAction("Dashboard", "User");
            }
            else
            {
                ViewBag.Title = "Login failed";
                return RedirectToAction("Login");
            }
            
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void getRequestWithToken(HttpWebRequest request)
        {
            Random rnd = new Random();
            var secret = rnd.Next(1, 50).ToString()
                         + rnd.Next(1, 20).ToString()
                         + rnd.Next(1, 100).ToString()
                         + rnd.Next(1, 10).ToString();

            Session["gentoken"] = secret;

            var postData = "api_key=243e8477";
            postData += "&api_secret=9602c2376b4ccc20";
            postData += "&to=0041796312888";
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
    }
}