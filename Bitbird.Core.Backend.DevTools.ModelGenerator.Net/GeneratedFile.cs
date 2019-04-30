namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    /// <summary>
    /// The result of the <see cref="ModelGenerator"/> for one file.
    /// Contains the file contents and the relative file name.
    /// </summary>
    public class GeneratedFile
    {
        /// <summary>
        /// The relative file name.
        /// </summary>
        public readonly string Filename;
        /// <summary>
        /// The content of the file.
        /// </summary>
        public readonly string Content;

        /// <summary>
        /// Constructs a <see cref="GeneratedFile"/>.
        /// For more info see the class documentation of <see cref="GeneratedFile"/>.
        /// </summary>
        /// <param name="filename">The relative file name.</param>
        /// <param name="content">The content of the file.</param>
        public GeneratedFile(string filename, string content)
        {
            Filename = filename;
            Content = content;
        }
    }
}