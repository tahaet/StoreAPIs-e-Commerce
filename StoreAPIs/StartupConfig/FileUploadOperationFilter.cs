using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using StoreModels.Dtos;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadParams = context
            .MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(ProductDto))
            .FirstOrDefault();

        if (fileUploadParams == null)
            return;

        operation.Parameters.Clear();

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["name"] = new OpenApiSchema { Type = "string" },
                            ["description"] = new OpenApiSchema { Type = "string" },
                            ["quantityInStock"] = new OpenApiSchema { Type = "integer" },
                            ["listPrice"] = new OpenApiSchema
                            {
                                Type = "number",
                                Format = "double"
                            },
                            ["price"] = new OpenApiSchema { Type = "number", Format = "double" },
                            ["price50"] = new OpenApiSchema { Type = "number", Format = "double" },
                            ["price100"] = new OpenApiSchema { Type = "number", Format = "double" },
                            ["categoryId"] = new OpenApiSchema { Type = "integer" },
                            ["productImages"] = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema { Type = "string", Format = "binary" }
                            }
                        },
                        Required = new HashSet<string>
                        {
                            "name",
                            "quantityInStock",
                            "listPrice",
                            "price",
                            "price50",
                            "price100",
                            "categoryId"
                        }
                    }
                }
            }
        };
    }
}
