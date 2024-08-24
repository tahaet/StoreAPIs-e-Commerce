using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreModels.Dtos
{
    public class StripeRequestDto
    {
        public string? StripeSessionUrl { get; set; }
        public string? StripeSessionId { get; set; }
        public string ApprovedUrl { get; set; }
        public string CancelUrl { get; set; }
        public ShoppingCartDto ShoppingCartDto { get; set; }
    }
}
