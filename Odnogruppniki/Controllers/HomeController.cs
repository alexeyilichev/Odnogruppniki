using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Odnogruppniki.Core;
using Odnogruppniki.Models;
using Odnogruppniki.Models.DBModels;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Odnogruppniki.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private DBContext _db;
        private UserManager _um;

        public HomeController() { }
        public HomeController(DBContext db, UserManager userManager)
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

        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.UserName = db.Users.FirstOrDefault().login; //test data
            return View("Index");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View("Login");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Login(string login, string password)
        {
            if (await um.IsValid(login, password))
            {
                var ident = new ClaimsIdentity(
                  new[] { 
                      // adding following 2 claim just for supporting default antiforgery provider
                      new Claim(ClaimTypes.NameIdentifier, login),

                      new Claim(ClaimTypes.Name, login)

                      //// optionally you could add roles if any
                      //new Claim(ClaimTypes.Role, "RoleName"),
                      //new Claim(ClaimTypes.Role, "AnotherRole")
                  },
                DefaultAuthenticationTypes.ApplicationCookie);

                HttpContext.GetOwinContext().Authentication.SignIn(
                   new AuthenticationProperties { IsPersistent = false }, ident);
                return Json(new { Success = true });
            } else
            {
                return Json(new { Success = false, Error = "Login or password are incorrect!" });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Groups()
        {
            return View("Index");
        }

        [HttpPost]
        public async Task SaveInfo(string photo, string aboutInfo)
        {
            var usr = await GetCurrentUser();
            var info = (await (from user in db.Users
                               where user.id == usr.id
                               join person in db.PersonalInfoes on user.id equals person.id_user
                               select person).FirstOrDefaultAsync());
            info.photo = photo;
            info.aboutinfo = aboutInfo;
            await db.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<ActionResult> Search()
        {
            var searchItems = await (from user in db.Users
                                     join person in db.PersonalInfoes
                                     on user.id equals person.id_user
                                     join univer in db.Universities
                                     on person.id_university equals univer.id
                                     join faculty in db.Faculties
                                     on person.id_faculty equals faculty.id
                                     join department in db.Departments
                                     on person.id_department equals department.id
                                     join grup in db.Groups
                                     on person.id_group equals grup.id
                                     select new SearchUserViewModel
                                     {
                                         id_user = user.id,
                                         photo = person.photo,
                                         name = person.name,
                                         id_university = person.id_university,
                                         university = univer.name,
                                         id_faculty = person.id_faculty,
                                         faculty = faculty.name,
                                         id_department = person.id_department,
                                         department = department.name,
                                         id_group = person.id_group,
                                         @group = grup.name
                                     }).ToListAsync();
            ViewBag.SearchUsers = searchItems;
            return View("Search");
        }

        [HttpGet]
        public async Task<ActionResult> SearchGroup()
        {
            var searchItems = await (from department in db.Departments
                                     join grup in db.Groups
                                     on department.id equals grup.id_department
                                     join faculty in db.Faculties
                                     on department.id_faculty equals faculty.id
                                     select new SearchGroupsViewModel
                                     {
                                         id_group = grup.id,
                                         name = grup.name,
                                         id_faculty = department.id_faculty,
                                         faculty = faculty.name,
                                         id_department = grup.id_department,
                                         department = department.name,
                                     }).ToListAsync();
            ViewBag.SearchGroups = searchItems;
            return View("SearchGroup");
        }

        [HttpGet]
        public ActionResult Settings()
        {
            return View("Index");
        }
        
        [HttpGet]
        public ActionResult LogoutPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.SignOut(
                    new AuthenticationProperties { IsPersistent = false }, User.Identity.AuthenticationType);
                return Json(new { Success = true });
            } else
            {
                return Json(new { Success = false, Error = "User is not login!" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Register(string login, string password, int id_group, int id_role)
        {
            if(await db.Users.FirstOrDefaultAsync(x => x.login == login) == null)
            {
                var newUser = new User
                {
                    login = login,
                    password = PasswordHash.GetPasswordHash(password),
                    id_group = id_group,
                    id_role = id_role
                };
                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                return Json(new { Success = true });
            }
            return Json(new { Success = false, Error = "This user already exists!" });
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