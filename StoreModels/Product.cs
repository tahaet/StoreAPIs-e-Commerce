using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace StoreModels
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public int QuantityInStock { get; set; }

        public double ListPrice { get; set; }

        public double Price { get; set; }

        public double Price50 { get; set; }

        public double Price100 { get; set; }
        public int CategoryId { get; set; }
        [ValidateNever]
        public Category Category { get; set; }
		[ValidateNever]

		public List<ProductImage> ProductImages { get; set; }
    }
}
