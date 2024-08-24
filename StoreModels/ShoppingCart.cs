using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StoreModels;

public class ShoppingCart
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; }

    [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000")]
    public int Count { get; set; }

    public string ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; }

    public double Price { get; set; }
    public string? CouponCode { get; set; }
}
