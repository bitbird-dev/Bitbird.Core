using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelSerializer
    {
        [NotNull] private readonly string path;

        [UsedImplicitly]
        public DataModelSerializer([NotNull] string path)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public Task<DataModelAssemblyInfo> ReadAsync()
        {
            DataModelAssemblyInfo model;

            using (var file = File.OpenText(path))
            {
                model = (DataModelAssemblyInfo)CreateSerializer().Deserialize(file, typeof(DataModelAssemblyInfo));
                file.Close();
            }

            return Task.FromResult(model);
        }

        public async Task WriteAsync(DataModelAssemblyInfo model)
        {
            using (var file = File.CreateText(path))
            {
                CreateSerializer().Serialize(file, model);
                await file.FlushAsync();
                file.Close();
            }
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private static JsonSerializer CreateSerializer()
        {
            var serializer = JsonSerializer.Create(JsonSerializerSettings);
            serializer.Formatting = Formatting.Indented;
            return serializer;
        }
    }
}
