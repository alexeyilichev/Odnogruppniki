using Microsoft.AspNet.Identity;
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
    public class GroupMessageController : Controller
    {
        private DBContext _db;
        private UserManager _um;

        public GroupMessageController() { }
        public GroupMessageController(DBContext db, UserManager userManager)
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

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var grup = await (from name in db.Groups
                              join user in db.Users
                              on name.id equals user.id
                              where name.id == user.id
                              select new GroupMessageViewModel
                              {
                                  id = name.id
                              }).FirstOrDefaultAsync();
            var date = DateTime.Now.AddDays(-1);
            var messages = await (from message in db.GroupMessages
                                  join group_in in db.Groups
                                  on message.id_in equals group_in.id
                                  join group_out in db.Groups
                                  on message.id_out equals group_out.id
                                  where message.id_in == grup.id || message.id_out == grup.id
                                  select new GroupMessageViewModel
                                  {
                                      id = message.id,
                                      id_in = message.id_in,
                                      id_out = message.id_out,
                                      message = message.message,
                                      date = message.date,
                                      name = group_out.name
                                  }).OrderByDescending(x => x.date).ToListAsync();
            messages.ForEach(x => x.dateString = date >= x.date ? string.Format("{0:dd/MM/yy}", x.date) : string.Format("{0:HH:mm:ss}", x.date));
            ViewBag.Messages = messages;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> OpenMessages(int par)
        {
            var grup = await (from name in db.Groups
                              join user in db.Users
                              on name.id equals user.id
                              where name.id == user.id
                              select new GroupMessageViewModel
                              {
                                  id = name.id
                              }).FirstOrDefaultAsync();
            var date = DateTime.Now.AddDays(-1);
            var messages = new List<GroupMessageViewModel>();
            if (par == 1)
            {
                messages.AddRange(await (from message in db.GroupMessages
                                         join group_in in db.Groups
                                         on message.id_in equals group_in.id
                                         join group_out in db.Groups
                                         on message.id_out equals group_out.id
                                         where message.id_out == grup.id
                                         select new GroupMessageViewModel
                                         {
                                             id = message.id,
                                             id_in = message.id_in,
                                             id_out = message.id_out,
                                             message = message.message,
                                             date = message.date,
                                             name = group_out.name
                                         }).OrderByDescending(x => x.date).ToListAsync());
            }
            else
            {
                messages.AddRange(await (from message in db.GroupMessages
                                         join group_in in db.Groups
                                         on message.id_in equals group_in.id
                                         join group_out in db.Groups
                                         on message.id_out equals group_out.id
                                         where message.id_in == grup.id
                                         select new GroupMessageViewModel
                                         {
                                             id = message.id,
                                             id_in = message.id_in,
                                             id_out = message.id_out,
                                             message = message.message,
                                             date = message.date,
                                             name = group_out.name
                                         }).OrderByDescending(x => x.date).ToListAsync());
            }
            messages.ForEach(x => x.dateString = date >= x.date ? string.Format("{0:dd/MM/yy}", x.date) : string.Format("{0:HH:mm:ss}", x.date));
            ViewBag.Messages = messages;
            return View("Index");
        }

        [HttpGet]
        public async Task<ActionResult> OpenMessage(int id)
        {
            var model = await (from message in db.GroupMessages
                               join group_in in db.Groups
                               on message.id_in equals group_in.id
                               join group_out in db.Groups
                               on message.id_out equals group_out.id
                               where message.id == id
                               select new GroupMessageViewModel
                               {
                                   id = message.id,
                                   id_in = message.id_in,
                                   id_out = message.id_out,
                                   message = message.message,
                                   date = message.date,
                                   name = group_out.name
                               }).FirstOrDefaultAsync();
            model.dateString = string.Format("{0:dd/MM/yy HH:mm:ss}", model.date);
            ViewBag.Message = model;
            return View("GroupMessage");
        }

        [HttpPost]
        public async Task SendMessage(int id_out, string message)
        {
            var grup = await (from name in db.Groups
                              join user in db.Users
                              on name.id equals user.id
                              where name.id == id_out
                              select new GroupMessageViewModel
                              {
                                  id = name.id
                              }).FirstOrDefaultAsync();
            var newMessage = new GroupMessage
                {
                    id_in = id_out,
                    id_out = grup.id,
                    message = message,
                    date = DateTime.UtcNow
                };
                db.GroupMessages.Add(newMessage);
                await db.SaveChangesAsync();
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