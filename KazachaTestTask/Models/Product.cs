using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KazachaTestTask.Models
{
    //[Serializable]
    public class Product
    {
        public int ProductId { get { return productId; } set { productId = value; } }
        [NonSerialized]
        private int productId;
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
