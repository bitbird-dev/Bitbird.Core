using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public abstract class BaseModelSerializer<TModel>
        where TModel : class
    {
        [NotNull] private readonly string path;

        [UsedImplicitly]
        protected BaseModelSerializer([NotNull] string path)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
        }

        [NotNull, ItemNotNull, UsedImplicitly]
        public Task<TModel> ReadAsync()
        {
            using (var file = File.OpenText(path))
            {
                var model = (TModel)CreateSerializer().Deserialize(file, typeof(TModel));
                file.Close();
                return Task.FromResult(model);
            }
        }

        [NotNull, UsedImplicitly]
        public async Task WriteAsync([NotNull] TModel model)
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
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting =  Formatting.Indented
        };
        [NotNull]
        private JsonSerializer CreateSerializer() => JsonSerializer.Create(jsonSerializerSettings);
    }
}
