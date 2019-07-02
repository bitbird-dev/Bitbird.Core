using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Web.JsonApi.Models;

namespace Bitbird.Core.Web.JsonApi
{
    public static class JsonApiErrorExtensions
    {
        public static JsonApiErrors ToJsonApiErrors(this Exception exc)
        {
            return new JsonApiErrors
            {
                Errors = exc.ToJsonApiErrorObjects()
            };
        }
        public static JsonApiErrorObject[] ToJsonApiErrorObjects(this Exception exc)
        {
            return exc.GetAggregatedExceptions().SelectMany(aggExc =>
            {
                switch (aggExc)
                {
                    case ApiErrorException aee:
                        return aee.ToJsonApiErrorObjects();
                }

                return new[]
                {
                    new JsonApiErrorObject
                    {
                        Id = Guid.NewGuid().ToString(),
                        Code = ((int) HttpStatusCode.InternalServerError).ToString(),
                        Detail = "Internal Server Error",
                        Status = ((int) HttpStatusCode.InternalServerError).ToString(),
                        Title = "Internal Server Error",
                        Source = new JsonApiErrorSource
                        {
                            Pointer = "/data",
                            Parameter = ""
                        },
                        Links = new JsonApiErrorLinks
                        {
                            About = ""
                        }
                    }
                };
            }).ToArray();
        }
        public static JsonApiErrorObject[] ToJsonApiErrorObjects(this ApiErrorException exc)
        {
            return exc.ApiErrors.Select(apiError =>
            {
                switch (apiError)
                {
                    case ApiAttributeError attributeError:
                        var attributeName = attributeError.AttributeName
                            .Replace('[', '/')
                            .Replace(']', '/')
                            .Replace('.', '/')
                            .Replace("//", "/")
                            .FromCamelCaseToJsonCamelCase();

                        return new JsonApiErrorObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Code = ((int)apiError.Type).ToString(),
                            Title = apiError.Title,
                            Detail = apiError.DetailMessage,
                            Status = typeof(ApiErrorType).GetMember(apiError.Type.ToString())?.FirstOrDefault()
                                         ?.GetCustomAttributes<HttpStatusCodeAttribute>().FirstOrDefault()?.StatusCode
                                         .ToString() ?? ((int)HttpStatusCode.InternalServerError).ToString(),
                            Source = new JsonApiErrorSource
                            {
                                Pointer = $"/data/attributes{attributeName}",
                                Parameter = ""
                            },
                            Links = new JsonApiErrorLinks
                            {
                                About = ""
                            }
                        };
                    case ApiParameterError parameterError:
                        return new JsonApiErrorObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Code = ((int)apiError.Type).ToString(),
                            Title = apiError.Title,
                            Detail = apiError.DetailMessage,
                            Status = typeof(ApiErrorType).GetMember(apiError.Type.ToString())?.FirstOrDefault()
                                         ?.GetCustomAttributes<HttpStatusCodeAttribute>().FirstOrDefault()?.StatusCode
                                         .ToString() ?? ((int)HttpStatusCode.InternalServerError).ToString(),
                            Source = new JsonApiErrorSource
                            {
                                Pointer = $"/parameters/{parameterError.ParameterName.FromCamelCaseToJsonCamelCase()}",
                                Parameter = ""
                            },
                            Links = new JsonApiErrorLinks
                            {
                                About = ""
                            }
                        };
                    default:
                        return new JsonApiErrorObject
                        {
                            Id = Guid.NewGuid().ToString(),
                            Code = ((int)apiError.Type).ToString(),
                            Title = apiError.Title,
                            Detail = apiError.DetailMessage,
                            Status = typeof(ApiErrorType).GetMember(apiError.Type.ToString())?.FirstOrDefault()
                                         ?.GetCustomAttributes<HttpStatusCodeAttribute>().FirstOrDefault()?.StatusCode
                                         .ToString() ?? ((int)HttpStatusCode.InternalServerError).ToString(),
                            Source = new JsonApiErrorSource
                            {
                                Pointer = "/data",
                                Parameter = ""
                            },
                            Links = new JsonApiErrorLinks
                            {
                                About = ""
                            }
                        };
                }
            }).ToArray();
        }
    }
}