using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EventBot {
    public struct Configuration {

        public Client client;
        public DriverConfig database;
        public DriverConfig mail;

        public Dictionary<string, string>[] modules;

        public struct Client {
            public string token;
            public string displayName;
        }

        public struct DriverConfig {
            public string driver;
            public Dictionary<string, string> driverOptions;
        }

        public bool GetModuleConfig<Config>(string name, out Config config) where Config : new() {
            Dictionary<string,string>? moduleBlock = modules.FirstOrDefault(x => x["name"] == name);
            if (moduleBlock == null) {
                config = new Config();
                return false;
            }

            Dictionary<string, string> dictionary = moduleBlock.Where(x => x.Key != "name").ToDictionary(x => x.Key, x => x.Value);
            string yaml = ToYaml(dictionary);
            config = FromYaml<Config>(yaml);
            return true;
        }

        public static Configuration LoadConfiguration(string? configPath = null) {

            string pathPrefix = "";
            if (configPath != null) {
                pathPrefix = configPath;
                if (!pathPrefix.EndsWith("/"))
                    pathPrefix += "/";
            }

            string configFile = pathPrefix + "config.yml";
            string credsFile = pathPrefix + "credentials.yml";

            if (!File.Exists(configFile))
                throw new FileNotFoundException("Config file could not be found at " + configFile);
            if (!File.Exists(credsFile))
                throw new FileNotFoundException("Credentials file could not be found at " + credsFile);
            
            Configuration config = FromYaml<Configuration>(File.ReadAllText(configFile));
            Configuration creds = FromYaml<Configuration>(File.ReadAllText(credsFile));

            Configuration loadConfiguration = ReflectionUtils.OverlayObjects<Configuration>(config, creds);
            return loadConfiguration;
        }
        
        public static string ToYaml(object obj) {
            ISerializer s = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            return s.Serialize(obj);
        }
        
        public static T FromYaml<T>(string str) {
            IDeserializer deserialiser = new DeserializerBuilder()
                                         .WithNamingConvention(UnderscoredNamingConvention.Instance)
                                         .Build();
            return deserialiser.Deserialize<T>(str);
        }
    }
}