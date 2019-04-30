using Microsoft.AspNet.Identity.Owin;
using Odnogruppniki.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Odnogruppniki.Controllers
{
    public class HomeController : Controller
    {
        private DBContext _db;

        public HomeController()
        {
            //_db = HttpContext.GetOwinContext().Get<DBContext>();
        }
        public HomeController(DBContext db)
        {
            _db = db;
        }

        public DBContext AppData
        {
            get
            {
                return _db ?? HttpContext.GetOwinContext().Get<DBContext>();
            }
            private set
            {
                _db = value;
            }
        }


        public ActionResult Index()
        {
            ViewBag.UserName = AppData.Users.FirstOrDefault().login; //test data
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}