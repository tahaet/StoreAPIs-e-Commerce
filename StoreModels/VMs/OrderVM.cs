using StoreModels.Dtos;

namespace StoreModels.Dtos
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail>? OrderDetails { get; set; }
    }
}
