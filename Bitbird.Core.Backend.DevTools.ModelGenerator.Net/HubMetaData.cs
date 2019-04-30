namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    public class HubMetaData
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly string name;
        public readonly HubServerMethodMetaData[] ServerMethods;
        public readonly HubClientMethodMetaData[] ClientMethods;

        public HubMetaData(string name, HubServerMethodMetaData[] serverMethods,
            HubClientMethodMetaData[] clientMethods)
        {
            this.name = name;
            ServerMethods = serverMethods;
            ClientMethods = clientMethods;
        }
    }
}