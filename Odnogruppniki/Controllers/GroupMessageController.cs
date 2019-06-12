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
using TeleSharp.TL;
using TLSharp.Core;

namespace Odnogruppniki.Controllers
{
    public class GroupMessageController : Controller
    {
        private DBContext _db;
        private UserManager _um;

        public TelegramClient client = new TelegramClient(668625, "0eb006301fad060c6212dda25f9c31e6", new WebSessionStore());

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
            var user = await GetCurrentUser();
            var grup = await (from gruup in db.Groups
                              join usr in db.Users
                              on gruup.id equals usr.id_group
                              where gruup.id == user.id_group
                              select new GroupMessageViewModel
                              {
                                  id = gruup.id
                              }).FirstOrDefaultAsync();
            var date = DateTime.Now.AddDays(-1);
            var messages = await (from message in db.GroupMessages
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
                                  }).OrderByDescending(x => x.date).ToListAsync();
            messages.ForEach(x => x.dateString = date >= x.date ? string.Format("{0:dd/MM/yy}", x.date) : string.Format("{0:HH:mm:ss}", x.date));
            ViewBag.Messages = messages;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> OpenMessages(int par)
        {
            var user = await GetCurrentUser();
            var grup = await (from gruup in db.Groups
                              join usr in db.Users
                              on gruup.id equals usr.id_group
                              where gruup.id == user.id_group
                              select new GroupMessageViewModel
                              {
                                  id = gruup.id
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
            var username = GetCurrentUserName();
            ViewBag.RoleName = (from usr in db.Users
                                where usr.login == username
                                join role in db.Roles
                                on usr.id_role equals role.id
                                select role.name).FirstOrDefault();
            ViewBag.IsAnswer = true;
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
            var user = await GetCurrentUser();
            var grup = await (from gruup in db.Groups
                              join usr in db.Users
                              on gruup.id equals usr.id_group
                              where gruup.id == user.id_group
                              select new GroupMessageViewModel
                              {
                                  id = gruup.id,
                                  name = gruup.name
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
            if (client.IsUserAuthorized())
            {
                var numbers = await (from usr in db.Users
                                     join gr in db.Groups
                                     on usr.id_group equals gr.id
                                     where gr.id == id_out
                                     join pi in db.PersonalInfoes
                                     on usr.id equals pi.id_user
                                     select pi.phone).ToListAsync();
                foreach (var number in numbers)
                {
                    await client.ConnectAsync();
                    var contacts = await client.GetContactsAsync();
                    var userID = contacts.Users.OfType<TLUser>().FirstOrDefault(x => x.Phone == number);
                    if (userID != null)
                    {
                        await client.SendMessageAsync(new TLInputPeerUser { UserId = userID.Id }, "You have a new message from " + grup.name + "!");
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult NewMessage(int id)
        {
            var username = GetCurrentUserName();
            ViewBag.RoleName = (from usr in db.Users
                                where usr.login == username
                                join role in db.Roles
                                on usr.id_role equals role.id
                                select role.name).FirstOrDefault();
            ViewBag.IsAnswer = false;
            ViewBag.Message = new GroupMessageViewModel { id_out = id, name = "name" };
            return View("GroupMessage");
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