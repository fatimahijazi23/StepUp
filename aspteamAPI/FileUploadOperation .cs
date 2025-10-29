using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace aspteamAPI
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile))
                .ToList();

            if (!fileParams.Any()) return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = context.MethodInfo.GetParameters()
                                .ToDictionary(
                                    p => p.Name!,
                                    p => p.ParameterType == typeof(IFormFile)
                                        ? new OpenApiSchema { Type = "string", Format = "binary" }
                                        : new OpenApiSchema { Type = "string" }
                                ),
                            Required = context.MethodInfo.GetParameters()
                                .Select(p => p.Name!)
                                .ToHashSet()
                        }
                    }
                }
            };
        }
    }
}