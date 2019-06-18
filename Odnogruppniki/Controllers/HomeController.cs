using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Odnogruppniki.Core;
using Odnogruppniki.Models;
using Odnogruppniki.Models.DBModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TeleSharp.TL;
using TLSharp.Core;

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

        public TelegramClient client = new TelegramClient(668625, "0eb006301fad060c6212dda25f9c31e6", new WebSessionStore());
        public static string hash = "";
        public TLUser user;
        public const string phone = "+79157566365";

        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            var user = await GetCurrentUser();
            if (user != null)
            {
                ViewBag.RoleName = (from usr in db.Users
                                    where usr.login == user.login
                                    join role in db.Roles
                                    on usr.id_role equals role.id
                                    select role.name).FirstOrDefault();
            }
            else
            {
                ViewBag.RoleName = "Guest";
            }
                var facultyforreg = (await (from faculty in db.Faculties
                                            select faculty).ToListAsync());
                var depforreg = (await (from department in db.Departments
                                        select department).ToListAsync());
                var groupsforreg = (await (from groups in db.Groups
                                           select groups).ToListAsync());
                ViewBag.Faculties = facultyforreg;
                ViewBag.Departments = depforreg;
                ViewBag.Groups = groupsforreg;
                return View("Index");
        }

        [HttpGet]
        public async Task<ActionResult> TLSettings()
        {
            if (!client.IsUserAuthorized())
            {
                return SendCode();
            } else
            {
                return await Index();
            }
        }

        private async Task Connect()
        {
            await client.ConnectAsync();
        }

        public ActionResult GetCode()
        {
            return View("GetCode");
        }

        public ActionResult SendCode()
        {
            return View("SendCode");
        }

        public async Task<ActionResult> SendCodeRequest()
        {
            await client.ConnectAsync();
            hash = await client.SendCodeRequestAsync(phone);
            return View("GetCode");
        }

        public async Task<ActionResult> MakeAuth(string code)
        {
            await client.ConnectAsync();
            user = await client.MakeAuthAsync(phone, hash, code);
            return await Index();
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
            }
            else
            {
                return Json(new { Success = false, Error = "Login or password are incorrect!" });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> Register()
        {
            var facultyforreg = (await (from faculty in db.Faculties
                                        select faculty).ToListAsync());
            var depforreg = (await (from department in db.Departments
                                    select department).ToListAsync());
            var groupsforreg = (await (from groups in db.Groups
                                       select groups).ToListAsync());
            ViewBag.Faculties = facultyforreg;
            ViewBag.Departments = depforreg;
            ViewBag.Groups = groupsforreg;
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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        public async Task<ActionResult> OpenProfile(int id)
        {
            var personalInfo = (await (from person in db.PersonalInfoes
                                       where person.id_user == id
                                       select person).FirstOrDefaultAsync());
            var user = (await db.Users.FirstOrDefaultAsync(x => x.id == id));
            var username = GetCurrentUserName();
            ViewBag.RoleName = (from usr in db.Users
                                where usr.login == username
                                join role in db.Roles
                                on usr.id_role equals role.id
                                select role.name).FirstOrDefault();
            ViewBag.Photo = personalInfo.photo;
            ViewBag.Name = personalInfo.name;
            ViewBag.University = (await db.Universities.FirstOrDefaultAsync(x => x.id == personalInfo.id_university)).name;
            ViewBag.Faculty = (await db.Faculties.FirstOrDefaultAsync(x => x.id == personalInfo.id_faculty)).name;
            ViewBag.Department = (await db.Departments.FirstOrDefaultAsync(x => x.id == personalInfo.id_department)).name; ;
            ViewBag.City = personalInfo.city;
            ViewBag.Role = (await db.Roles.FirstOrDefaultAsync(x => x.id == user.id_role)).name;
            ViewBag.AboutInfo = personalInfo.aboutinfo;
            ViewBag.UserId = id;
            ViewBag.MyPage = false;
            ViewBag.Roles = (await db.Roles.ToListAsync());
            ViewBag.Group = (await db.Groups.FirstOrDefaultAsync(x => x.id == user.id_group)).name;
            return View("/Views/Personal/PersonalInfo.cshtml");
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
            }
            else
            {
                return Json(new { Success = false, Error = "User is not login!" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Register(string login, string password, string fio, string phone, int id_faculty, int id_department, int id_group)
        {
            if (await db.Users.FirstOrDefaultAsync(x => x.login == login) == null)
            {
                var newUser = new User
                {
                    login = login,
                    password = PasswordHash.GetPasswordHash(password),
                    id_group = id_group,
                    id_role = 2
                };
                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                var user = (await (from users in db.Users
                                   where users.login == login
                                   select users).FirstOrDefaultAsync());
                var newPersonalInfo = new PersonalInfo
                {
                    name = fio,
                    id_university = 2,
                    id_faculty = id_faculty,
                    id_department = id_department,
                    id_role = 2,
                    id_group = id_group,
                    id_user = user.id,
                    phone = phone,
                    city = "Владимир",
                    aboutinfo = "О себе",
                    photo = "/Content/defaultphoto.jpg"
                };
                db.PersonalInfoes.Add(newPersonalInfo);
                await db.SaveChangesAsync();
                return Json(new { Success = true });
            }
            return Json(new { Success = false, Error = "This user already exists!" });
        }

        [HttpPost]
        public async Task<ActionResult> AddFaculty(string name)
        {
            if (await db.Faculties.FirstOrDefaultAsync(x => x.name == name) == null)
            {
                var newFaculty = new Faculty
                {
                    name = name,
                    id_university = 2
                };
                db.Faculties.Add(newFaculty);
                await db.SaveChangesAsync();
                return Json(new { Success = true });
            }
            return Json(new { Success = false, Error = "This faculty already exists!" });
        }
        [HttpPost]
        public async Task<ActionResult> AddDepartment(string name, int id_faculty)
        {
            if (await db.Faculties.FirstOrDefaultAsync(x => x.name == name) == null)
            {
                var newDepartment = new Department
                {
                    name = name,
                    id_university = 2,
                    id_faculty = id_faculty
                };
                db.Departments.Add(newDepartment);
                await db.SaveChangesAsync();
                return Json(new { Success = true });
            }
            return Json(new { Success = false, Error = "This department already exists!" });
        }

        [HttpPost]
        public async Task<ActionResult> AddGroup(string name, int id_department)
        {
            if (await db.Groups.FirstOrDefaultAsync(x => x.name == name) == null)
            {
                var newGroup = new Group
                {
                    name = name,
                    id_department = id_department
                };
                db.Groups.Add(newGroup);
                await db.SaveChangesAsync();
                return Json(new { Success = true });
            }
            return Json(new { Success = false, Error = "This group already exists!" });
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