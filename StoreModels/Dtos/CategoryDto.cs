using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreModels.Dtos
{
    public class CategoryDto
    {
        public string Name { get; set; }

        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
