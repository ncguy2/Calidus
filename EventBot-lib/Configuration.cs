using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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