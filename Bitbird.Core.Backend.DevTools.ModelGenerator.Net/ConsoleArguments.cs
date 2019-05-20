using System;
using Bitbird.Core.CommandLineArguments;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    /// <summary>
    /// Supported console arguments.
    ///
    /// Can be passed to the application by calling the exe with the given properties.
    /// 
    /// E.g.: <c>Bitbird.Core.Backend.DevTools.ModelGenerator.Net.exe -TargetDirectory models -TargetFile ..\..\..\BackRohr.Web.Api\Doc\Interface\resources\ember-js-models.zip -TargetFormat Js -DocDirectory ..\..\..\BackRohr.Web.Api\bin</c>
    /// </summary>
    public class ConsoleArguments
    {
        /// <summary>
        /// The target directory where the results are stored before creating the zip file from it.
        /// Is not deleted after the zip file is created.
        /// </summary>
        [CommandLineArg]
        public string TargetDirectory { get; set; } = @"models";

        /// <summary>
        /// The zip file where the result is stored.
        /// Relative to the WD.
        /// </summary>
        [CommandLineArg]
        public string TargetFile { get; set; } = $@"models-{DateTime.Now:yyyy-MM-ddTHH-mm}.zip";

        /// <summary>
        /// The target format.
        /// Case-sensitive.
        ///
        /// Used as prefix for resources.
        /// 
        /// E.g.:
        /// - Js
        /// - Cs
        /// </summary>
        [CommandLineArg]
        public string TargetFormat { get; set; } = @"Js";

        /// <summary>
        /// The directory to look for documentation xml files.
        /// </summary>
        [CommandLineArg]
        public string DocDirectory { get; set; } = @"..\..\..\BackRohr.Web.Api\bin";

        /// <summary>
        /// Whether or not console outputs should indicate the progress.
        /// True for silence.
        /// </summary>
        [CommandLineArg]
        public bool Silent { get; set; } = false;
    }
}