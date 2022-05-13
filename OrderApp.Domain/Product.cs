using System;
using System.Collections.Generic;

namespace OrderApp.Domain
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Stock { get; set; }
        public int Price { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
