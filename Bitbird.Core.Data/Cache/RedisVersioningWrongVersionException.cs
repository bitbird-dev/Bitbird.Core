namespace Bitbird.Core.Data.Cache
{
    public class RedisVersioningWrongVersionException : RedisVersioningException
    {
        public readonly uint CurrentVersion;
        public readonly uint SerializedVersion;

        public RedisVersioningWrongVersionException(uint currentVersion, uint serializedVersion)
            : base($"{nameof(CurrentVersion)}: {currentVersion}, {nameof(SerializedVersion)}: {serializedVersion}")
        {
            CurrentVersion = currentVersion;
            SerializedVersion = serializedVersion;
        }
    }
}