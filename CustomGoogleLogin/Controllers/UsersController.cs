using CustomGoogleLogin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustomGoogleLogin.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        // GET: Users
        public ActionResult Index()
        {
            WebEntities db = new WebEntities();
            var list = db.UserAccounts.ToList();
            return View(list);
        }
    }
}