using dotnet_api_example.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace dotnet_api_example.Swagger
{
    public class SwaggerDocument
    {
        public static List<OpenApiInfo> Info = new List<OpenApiInfo>{
                new OpenApiInfo { 
                    Title = "dotnet_api_example", 
                    Version = "v1",
                    Description = "Hello world",
                },
            };
    }

    internal static class OperationFilterContextExtensions
    {
        internal static Boolean TryGetAuthorizeAttribute(this OperationFilterContext context, out AuthorizeAttribute found)
        {
            found = null;
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor)
            {
                found = ((ControllerActionDescriptor)context.ApiDescription.ActionDescriptor).ControllerTypeInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().FirstOrDefault();
                if (found != null)
                {
                    return true;
                }
            }
            context.ApiDescription.TryGetMethodInfo(out var methodInfo);
            if (methodInfo.MemberType == MemberTypes.Method)
            {
                found = methodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().FirstOrDefault();
                if (found != null)
                {
                    return true;
                }
                found = methodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().FirstOrDefault();
                if (found != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class AuthOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorizeAttribute = context.TryGetAuthorizeAttribute(out var authorizeAttribute);
            if (!hasAuthorizeAttribute)
                return;

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }
            var schemes = authorizeAttribute.AuthenticationSchemes.Split(",");

            if (schemes.Contains(APIAuthenticationScheme.AuthenticationSchemeName))
            {
                operation.Security.Add(new OpenApiSecurityRequirement {
                { new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = APIAuthenticationScheme.AuthenticationSchemeName
                        }
                    },
                    new List<String>(){}
                    }
                });
            }
        }
    }
}
