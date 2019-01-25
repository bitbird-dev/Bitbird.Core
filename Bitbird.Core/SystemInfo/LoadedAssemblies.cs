using System;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.SystemInfo
{
    public static class LoadedAssemblies
    {
        public static string ReportLoadedAssemblies()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Loaded assemblies:");
            foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AssemblyName assemblyName;
                try
                {
                    assemblyName = loadedAssembly.GetName();
                }
                catch (Exception e)
                {
                    sb.AppendFormat("  EXCEPTION {0}({1})", e.GetType(), e.Message);
                    sb.AppendLine();
                    continue;
                }

                string name;
                string version;
                string location;

                try
                {
                    name = assemblyName.Name;
                }
                catch (Exception e)
                {
                    name = $"EXCEPTION {e.GetType()}({e.Message})";
                }
                try
                {
                    version = assemblyName.Version.ToString();
                }
                catch (Exception e)
                {
                    version = $"EXCEPTION {e.GetType()}({e.Message})";
                }
                try
                {
                    location = loadedAssembly.Location;
                }
                catch (Exception e)
                {
                    location = $"EXCEPTION {e.GetType()}({e.Message})";
                }

                sb.AppendFormat("  {0}:v{1} ({2})", name, version, location);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
