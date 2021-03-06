﻿using Microsoft.AspNet.Identity.Owin;
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
using TLSharp.Core;
using TLSharp.Core.Requests;
using TLSharp.Core.Utils;
using TLSharp.Core.Network;
using TeleSharp.TL;
using TLSharp.Core.MTProto;
using TeleSharp.TL.Messages;

namespace Odnogruppniki.Controllers
{
    [Authorize]
    public class PersonalMessageController : Controller
    {
        private DBContext _db;
        private UserManager _um;
        public TelegramClient client = new TelegramClient(668625, "0eb006301fad060c6212dda25f9c31e6", new WebSessionStore());

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

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var username = GetCurrentUserName();
            ViewBag.RoleName = (from usr in db.Users
                                where usr.login == username
                                join role in db.Roles
                                on usr.id_role equals role.id
                                select role.name).FirstOrDefault();
            var user = await GetCurrentUser();
            var date = DateTime.Now.AddDays(-1);
            var messages = await (from message in db.PersonalMessages
                                  join user_in in db.Users
                                  on message.id_in equals user_in.id
                                  join user_out in db.Users
                                  on message.id_out equals user_out.id
                                  join person_in in db.PersonalInfoes
                                  on user_in.id equals person_in.id_user
                                  join person_out in db.PersonalInfoes
                                  on user_out.id equals person_out.id_user
                                  where message.id_in == user.id || message.id_out == user.id
                                  select new PersonalMessageViewModel
                                  {
                                      id = message.id,
                                      id_in = message.id_in,
                                      id_out = message.id_out,
                                      message = message.message,
                                      name_in = person_in.name,
                                      name_out = person_out.name,
                                      photo_in = person_in.photo,
                                      photo_out = person_out.photo,
                                      date = message.date
                                  }).OrderByDescending(x => x.date).ToListAsync();
            messages.ForEach(x => x.dateString = date >= x.date ? string.Format("{0:dd/MM/yy}", x.date) : string.Format("{0:HH:mm:ss}", x.date));
            ViewBag.Messages = messages;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> OpenMessages(int par)
        {
            var username = GetCurrentUserName();
            ViewBag.RoleName = (from usr in db.Users
                                where usr.login == username
                                join role in db.Roles
                                on usr.id_role equals role.id
                                select role.name).FirstOrDefault();
            var user = await GetCurrentUser();
            var date = DateTime.Now.AddDays(-1);
            var messages = new List<PersonalMessageViewModel>();
            if (par == 1)
            {
                messages.AddRange(await (from message in db.PersonalMessages
                                         join user_in in db.Users
                                         on message.id_in equals user_in.id
                                         join user_out in db.Users
                                         on message.id_out equals user_out.id
                                         join person_in in db.PersonalInfoes
                                         on user_in.id equals person_in.id_user
                                         join person_out in db.PersonalInfoes
                                         on user_out.id equals person_out.id_user
                                         where message.id_out == user.id
                                         select new PersonalMessageViewModel
                                         {
                                             id = message.id,
                                             id_in = message.id_in,
                                             id_out = message.id_out,
                                             message = message.message,
                                             name_in = person_in.name,
                                             name_out = person_out.name,
                                             photo_in = person_in.photo,
                                             photo_out = person_out.photo,
                                             date = message.date
                                         }).OrderByDescending(x => x.date).ToListAsync());
            }
            else
            {
                messages.AddRange(await (from message in db.PersonalMessages
                                         join user_in in db.Users
                                         on message.id_in equals user_in.id
                                         join user_out in db.Users
                                         on message.id_out equals user_out.id
                                         join person_in in db.PersonalInfoes
                                         on user_in.id equals person_in.id_user
                                         join person_out in db.PersonalInfoes
                                         on user_out.id equals person_out.id_user
                                         where message.id_in == user.id
                                         select new PersonalMessageViewModel
                                         {
                                             id = message.id,
                                             id_in = message.id_in,
                                             id_out = message.id_out,
                                             message = message.message,
                                             name_in = person_in.name,
                                             name_out = person_out.name,
                                             photo_in = person_in.photo,
                                             photo_out = person_out.photo,
                                             date = message.date
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
            var model = await (from message in db.PersonalMessages
                               join user_in in db.Users
                               on message.id_in equals user_in.id
                               join user_out in db.Users
                               on message.id_out equals user_out.id
                               join person_in in db.PersonalInfoes
                               on user_in.id equals person_in.id_user
                               join person_out in db.PersonalInfoes
                               on user_out.id equals person_out.id_user
                               where message.id == id
                               select new PersonalMessageViewModel
                               {
                                   id = message.id,
                                   id_in = message.id_in,
                                   id_out = message.id_out,
                                   message = message.message,
                                   name_in = person_in.name,
                                   name_out = person_out.name,
                                   photo_in = person_in.photo,
                                   photo_out = person_out.photo,
                                   date = message.date
                               }).FirstOrDefaultAsync();
            model.dateString = string.Format("{0:dd/MM/yy HH:mm:ss}", model.date);
            ViewBag.Message = model;
            var userName = GetCurrentUserName();
            ViewBag.User = await (from user in db.Users.Where(x => x.login == userName)
                                  join person in db.PersonalInfoes on user.id equals person.id_user
                                  select person.name).FirstOrDefaultAsync();
            ViewBag.IsAnswer = true;
            return View("PersonalMessage");
        }

        [HttpPost]
        public async Task SendMessage(int id_out, string message)
        {
            var user = await GetCurrentUser();
            ViewBag.RoleName = (from usr in db.Users
                                where usr.login == user.login
                                join role in db.Roles
                                on usr.id_role equals role.id
                                select role.name).FirstOrDefault();
            var userPersonal = await (from u in db.Users
                                      join pi in db.PersonalInfoes
                                      on u.id equals pi.id_user
                                      where u.id == user.id
                                      select pi.name).FirstOrDefaultAsync();
            if (user.id != id_out)
            {
                var newMessage = new PersonalMessage
                {
                    id_in = id_out,
                    id_out = user.id,
                    message = message,
                    date = DateTime.UtcNow
                };
                db.PersonalMessages.Add(newMessage);
                await db.SaveChangesAsync();
            }
            if (client.IsUserAuthorized())
            {
                var number = await (from usr in db.Users
                                    where usr.id == id_out
                                    join pi in db.PersonalInfoes
                                    on usr.id equals pi.id_user
                                    select pi.phone).FirstOrDefaultAsync();
                await client.ConnectAsync();
                var contacts = await client.GetContactsAsync();
                var userID = contacts.Users.OfType<TLUser>().FirstOrDefault(x => x.Phone == number);
                if (userID != null)
                {
                    await client.SendMessageAsync(new TLInputPeerUser { UserId = userID.Id }, "You have a new message from " + userPersonal + "!");

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
            ViewBag.Message = new PersonalMessageViewModel { id_out = id, name_out = "Other user" };
            return View("PersonalMessage");
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