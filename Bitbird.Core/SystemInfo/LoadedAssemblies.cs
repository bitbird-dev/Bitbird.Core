using System;
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
                var assemblyName = loadedAssembly.GetName();
                sb.AppendFormat("  {0}:v{1} ({2})", assemblyName.Name, assemblyName.Version, loadedAssembly.Location);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
