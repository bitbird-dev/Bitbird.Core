using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class ApiModelSerializer
    {
        [NotNull] private readonly string path;

        [UsedImplicitly]
        public ApiModelSerializer([NotNull] string path)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
        }

        [NotNull, ItemNotNull, UsedImplicitly]
        public Task<ApiModelAssemblyInfo> ReadAsync()
        {
            ApiModelAssemblyInfo model;

            using (var file = File.OpenText(path))
            {
                model = (ApiModelAssemblyInfo)CreateSerializer().Deserialize(file, typeof(ApiModelAssemblyInfo));
                file.Close();
            }

            return Task.FromResult(model);
        }

        [NotNull, UsedImplicitly]
        public async Task WriteAsync([NotNull] ApiModelAssemblyInfo model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            using (var file = File.CreateText(path))
            {
                CreateSerializer().Serialize(file, model);
                await file.FlushAsync();
                file.Close();
            }
        }

        [NotNull]
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        [NotNull]
        private static JsonSerializer CreateSerializer()
        {
            var serializer = JsonSerializer.Create(JsonSerializerSettings);
            serializer.Formatting = Formatting.Indented;
            return serializer;
        }
    }
}
