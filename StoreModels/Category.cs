using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StoreModels;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
