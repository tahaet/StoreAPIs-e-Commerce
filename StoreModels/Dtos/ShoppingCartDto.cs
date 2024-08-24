using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreModels.Dtos
{
    public class ShoppingCartDto
    {
        public IEnumerable<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
        public OrderHeader OrderHeader { get; set; }
    }
}
