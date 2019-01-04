using Bitbird.Core.Exceptions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Bitbird.Core.Expressions
{
    public static class PropertiesHelper
    {
        public static Func<T, object> CompileDottedPropertyGetter<T>(string dottedProperty)
        {
            try
            {
                var path = dottedProperty.Split('.');

                var param = Expression.Parameter(typeof(T), "x");

                Expression body = param;
                var bodyType = typeof(T);

                foreach (var node in path)
                {
                    var property = bodyType.GetProperties().Single(p => p.Name.ToUpper().Equals(node.ToUpper()));

                    body = Expression.Property(body, property);
                    bodyType = property.PropertyType;
                }

                body = Expression.Convert(body, typeof(object));

                var lambda = Expression.Lambda<Func<T, object>>(body, param);

                return lambda.Compile();
            }
            catch(Exception e)
            {
                throw new PropertyGetterCompilationException($"Could not compile property '{dottedProperty}'.", e);
            }
        }
    }
}
