namespace Bitbird.Core.Backend.DevTools.RestDoc.Net
{
    public class ControllerMethodInfo
    {
        public string Route { get; }
        public bool QueryParams { get; }
        public string Parameter { get; }
        public string Returns { get; }
        public string Cmd { get; }

        public ControllerMethodInfo(string route, bool queryParams, string parameter, string returns, string cmd)
        {
            Route = route;
            QueryParams = queryParams;
            Parameter = parameter;
            Returns = returns;
            Cmd = cmd;
        }
    }
}
