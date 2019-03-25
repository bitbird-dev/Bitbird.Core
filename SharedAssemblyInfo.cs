using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Bitbird GmbH")]
[assembly: AssemblyProduct("Bitbird.Core")]
[assembly: AssemblyCopyright("Copyright © Bitbird GmbH 2019")]
[assembly: AssemblyTrademark("")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]
[assembly: AssemblyVersion(Bitbird.Core.SharedAssemblyVersion.VersionNumberString)]
[assembly: AssemblyInformationalVersion(Bitbird.Core.SharedAssemblyVersion.VersionNumberString)] // a.k.a. "Product version"
[assembly: AssemblyFileVersion(Bitbird.Core.SharedAssemblyVersion.VersionNumberString)]

namespace Bitbird.Core
{
    internal static class SharedAssemblyVersion
    {
        public const string VersionNumberString = "1.1.41.0"; 
    }
}