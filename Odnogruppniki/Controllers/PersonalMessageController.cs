using Microsoft.AspNet.Identity.Owin;
using Odnogruppniki.Core;
using Odnogruppniki.Models;
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
    public class PersonalMessageController : Controller
    {
        private DBContext _db;
        private UserManager _um;

        public PersonalMessageController() { }
        public PersonalMessageController(DBContext db, UserManager userManager)
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

        // GET: PersonalMessage
        public async Task<ActionResult> Index()
        {
            var user = await GetCurrentUser();
            var date = DateTime.Now.AddDays(-1);
            var messages = await (from message in db.PersonalMessages
                            join user_in in db.Users
                            on message.id_in equals user_in.id
                            join user_out in db.Users
                            on message.id_out equals user_out.id
                            where message.id_in == user.id || message.id_out == user.id
                            select new PersonalMessageViewModel
                            {
                                id_in = message.id_in,
                                id_out = message.id_out,
                                message = message.message,
                                name_in = user_in.login,
                                name_out = user_out.login,
                                date = message.date
        }).ToListAsync();
            messages.ForEach(x => x.dateString = date >= x.date ? string.Format("{0:dd/MM/yyyy}", x.date) : string.Format("{0:HH:mm:ss}", x.date));
            ViewBag.Messages = messages;
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