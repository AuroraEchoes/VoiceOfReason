using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VoiceOfReason
{
    public static class Configuration
    {
        public static Config Config;

        static Configuration()
        {
            string configFile = File.ReadAllText("config.yaml");
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            Config = deserializer.Deserialize<Config>(configFile);
        }
    }
}
