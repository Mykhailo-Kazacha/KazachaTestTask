using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KazachaTestTask.Models;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;


namespace KazachaTestTask.Controllers
{
    public class OrdersController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        //
        // GET: /Orders/
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.Product).Include(o => o.Customer);
            return View(orders.OrderBy(s => s.Customer.Name).ToList());
        }

        public ActionResult OrdersData(string sortParam)
        {
            //Посмотреть предыдущий параметр сортировки (по возрастанию или убыванию)
            string sortOrder = TempData.Peek("sortOrder") as string;
            //Определить по какому свойству и как сортировать
            string sort = sortParam + " " + sortOrder;

            var orders = db.Orders.Include(o => o.Product).Include(o => o.Customer);
            string searchString = TempData.Peek("searchString") as string;

            //определение того, что надо сортировать (если производился поиск)
            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(s => s.Customer.Name.ToUpper().Contains(searchString.ToUpper()));
            }

            switch (sort)
            {
                case "Name desc":
                    orders = orders.OrderByDescending(s => s.Customer.Name);
                    break;
                case "Product ":
                    orders = orders.OrderBy(s => s.Product.Name);
                    break;
                case "Product desc":
                    orders = orders.OrderByDescending(s => s.Product.Name);
                    break;
                case "Date ":
                    orders = orders.OrderBy(s => s.Date);
                    break;
                case "Date desc":
                    orders = orders.OrderByDescending(s => s.Date);
                    break;
                default:
                    orders = orders.OrderBy(s => s.Customer.Name);
                    break;
            }
            TempData["sortOrder"] = String.IsNullOrEmpty(sortOrder) ? "desc" : "";
            return PartialView(orders.ToList());
        }

        [HttpPost]
        public ActionResult Search(string searchString)
        {
            var orders = db.Orders.Include(o => o.Product).Include(o => o.Customer);
            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(s => s.Customer.Name.ToUpper().Contains(searchString.ToUpper()));
            }
            //запоминание поискового запроса
            TempData["searchString"] = searchString;
            return PartialView("OrdersData",orders.ToList());
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (upload != null)
            {
                string fileName = Path.GetFileName(upload.FileName);
                string extenison = Path.GetExtension(upload.FileName);
                Order order = null;

                // определение формата файла (и соответствующего сериализатора). Возможен вариант xml или json
                //В файле должен находиться только один объект типа Order.
                switch (extenison)
                {
                    case ".json":
                        {
                            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Order));
                            order = (Order)deserializer.ReadObject(upload.InputStream);

                        }
                        break;
                    case ".xml":
                        {
                            //XmlSerializer deserializer = new XmlSerializer(typeof(Order));
                            XmlSerializer deserializer = new XmlSerializer(typeof(Order));
                            order = (Order)deserializer.Deserialize(upload.InputStream);
                        }
                        break;
                    default:
                        { //выдать сообщение о неверном формате файла
                            ViewBag.Message = "Неверный формат файла";
                            return View("Error");
                        }
                }

                //проверка наличия пользователя (определяется по мылу) и продукта (по соответствию названия и цены) в БД и добавление в БД
                try
                {
                    
                    var tmp = db.Customers.Where(s => s.Email.Equals(order.Customer.Email));
                    Customer customer = new Customer();
                    if (tmp.ToList().Count > 0)
                    {
                        customer = tmp.ToList()[0];
                    }
                    else
                    {
                        customer.Email = order.Customer.Email;
                        customer.Name = order.Customer.Name;
                    }


                    var tmp1 = db.Products.Where(s => s.Name.Equals(order.Product.Name) 
                                && s.Price.Equals(order.Product.Price));
                    Product product = null;
                    if (tmp1.ToList().Count > 0)
                    {
                        product = tmp1.ToList()[0];
                    }


                    if (product != null)
                    {
                        order.Customer = customer;
                        order.Product = product;
                        order.CustomerId = customer.CustomerId;
                        order.ProductId = product.ProductId;
                        order.Date = DateTime.Now;
                        
                        db.Orders.Add(order);
                        var orders = db.Orders;
                        db.SaveChanges();
                        var customers = db.Customers;
                    }
                }
                catch
                {
                    //ошибка подключения к бд
                    ViewBag.Message = "Ошибка при чтении/записи файла";
                    return View("Error");
                }

            }
            else
            {
                ViewBag.Message = "Файл не передан на сервер";
                return View("Error");
            }
            return View("Index", db.Orders.OrderBy(s => s.Customer.Name).ToList());
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}