using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreModels.Dtos
{
    public class OrderDto
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail> OrderDetails { get; set; }
    }
}
