using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Odnogruppniki.Core;
using Odnogruppniki.Models.DBModels;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Odnogruppniki.Controllers
{
    public class HomeController : Controller
    {
        private DBContext _db;
        private UserManager _um;

        public HomeController(){}
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


        public ActionResult Index()
        {
            ViewBag.UserName = db.Users.FirstOrDefault().login; //test data
            return View("Index");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View("Login");
        }

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
                return Json(new { Success = true }); // auth succeed 
            } else
            {
                return Json(new { Success = false, Error = "Login or password are incorrect!" });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.User = GetCurrentUserName();
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PersonalMessage()
        {
            return View("Index");
        }

        [HttpGet]
        public ActionResult Groups()
        {
            return View("Index");
        }

        [HttpGet]
        public ActionResult Search()
        {
            return View("Index");
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