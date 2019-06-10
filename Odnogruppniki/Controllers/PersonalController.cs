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
    [Authorize]
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

        public async Task<ActionResult> PersonalInfo(int? id)
        {
            var user = await GetCurrentUser();
            var personalInfo = new PersonalInfo();
            if (!id.HasValue)
            {
                personalInfo = await db.PersonalInfoes.FirstOrDefaultAsync(x => x.id_user == user.id);
            } else
            {
                personalInfo = await db.PersonalInfoes.FirstOrDefaultAsync(x => x.id_user == id);
            }
            ViewBag.Photo = personalInfo.photo;
            ViewBag.Name = personalInfo.name;
            ViewBag.University = (await db.Universities.FirstOrDefaultAsync(x=> x.id == personalInfo.id_university)).name;
            ViewBag.Faculty = (await db.Faculties.FirstOrDefaultAsync(x => x.id == personalInfo.id_faculty)).name;
            ViewBag.Department = (await db.Departments.FirstOrDefaultAsync(x => x.id == personalInfo.id_department)).name; ;
            ViewBag.City = personalInfo.city;
            ViewBag.Role = (await db.Roles.FirstOrDefaultAsync(x => x.id == personalInfo.id_role)).name;
            ViewBag.AboutInfo = personalInfo.aboutinfo;
            ViewBag.UserId = user.id;
            ViewBag.MyPage = true;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            ViewBag.UserId = (await GetCurrentUser()).id;
            ViewBag.Universities = await db.Universities.ToListAsync();
            ViewBag.Faculties = await db.Faculties.ToListAsync();
            ViewBag.Departments = await db.Departments.ToListAsync();
            ViewBag.Groups = await db.Groups.ToListAsync();
            var user = await GetCurrentUser();
            ViewBag.User = (await (from usr in db.Users
                                   join person in db.PersonalInfoes on usr.id equals person.id_user
                                   where usr.id == user.id
                                   select person).FirstOrDefaultAsync());
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