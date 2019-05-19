using Microsoft.AspNet.Identity.Owin;
using Odnogruppniki.Core;
using Odnogruppniki.Models.DBModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Odnogruppniki.Controllers
{
    public class PersonalController : Controller
    {

        private DBContext _db;
        private UserManager _um;

        public PersonalController() { }
        public PersonalController(DBContext db, UserManager userManager)
        {
            _db = db;
        }

        public DBContext db
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

        public UserManager um
        {
            get
            {
                return _um ?? HttpContext.GetOwinContext().Get<UserManager>();
            }
            private set
            {
                _um = value;
            }
        }
        // GET: Personal
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> PersonalInfo()
        {
            var user = await GetCurrentUser();
            var personalInfo = await db.PersonalInfoes.FirstOrDefaultAsync(x => x.id_user == user.id);
            ViewBag.Photo = personalInfo.photo;
            ViewBag.Name = personalInfo.name;
            ViewBag.University = (await db.Universities.FirstOrDefaultAsync(x=> x.id == personalInfo.id_university)).name; //test data
            ViewBag.Faculty = (await db.Faculties.FirstOrDefaultAsync(x => x.id == personalInfo.id_faculty)).name; //test data
            ViewBag.Department = (await db.Departments.FirstOrDefaultAsync(x => x.id == personalInfo.id_department)).name; ; //test data
            ViewBag.City = personalInfo.city; //test data
            ViewBag.Role = (await db.Roles.FirstOrDefaultAsync(x => x.id == personalInfo.id_role)).name; //test data
            ViewBag.AboutInfo = personalInfo.aboutinfo;
            return View();
        }

        private string GetCurrentUserName()
        {
            return HttpContext.GetOwinContext().Authentication.User.Identity.Name;
        }

        private async Task<User> GetCurrentUser()
        {
            var name = GetCurrentUserName();
            return await db.Users.FirstOrDefaultAsync(x => x.login == name);
        }
    }
}