using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KazachaTestTask.Models;

namespace KazachaTestTask.Controllers
{
    public class CustomersController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        //
        // GET: /Customers/

        public ActionResult Index()
        {
            return View(db.Customers.ToList());
        }

        //
        // GET: /Customers/Details/5

        public ActionResult Details(int id = 0)
        {
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}