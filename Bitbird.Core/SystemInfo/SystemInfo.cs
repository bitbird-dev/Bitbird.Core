using System;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.SystemInfo
{
    public static class SystemInfo
    {
        public static string ReportPrimitiveSystemInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Primitive Info:");

            foreach (var propertyInfo in typeof(Environment).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (!propertyInfo.CanRead)
                    continue; 

                try
                {
                    sb.AppendFormat("  {0}: {1}", propertyInfo.Name, propertyInfo.GetValue(null));
                }
                catch (Exception e)
                {
                    sb.AppendFormat("  {0}: EXCEPTION {2}({1})", propertyInfo.Name, e.GetType(), e.Message);
                }
                sb.AppendLine();
            }

            foreach (var fieldInfo in typeof(Environment).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    sb.AppendFormat("  {0}: {1}", fieldInfo.Name, fieldInfo.GetValue(null));
                }
                catch (Exception e)
                {
                    sb.AppendFormat("  {0}: EXCEPTION {2}({1})", fieldInfo.Name, e.GetType(), e.Message);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}