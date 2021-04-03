using System.Text.Json.Serialization;
using RedNX.Config;

namespace HtcPlugin.DeSServer.Core {
    public class DeSConfig : BaseConfig {

        [JsonPropertyName("des-server")]
        public DeSServerConfig DeSServer { get; set; }

        [JsonPropertyName("db")]
        public DatabaseConfig Db { get; set; }

        public DeSConfig() : base(1) {
            DeSServer = new DeSServerConfig {

            };
            Db = new DatabaseConfig {
                Host = "127.0.0.1",
                Port = 3306,
                Database = "des",
                Username = "root",
                Password = "root"
            };
            Version = 1;
        }
    }

    public class DeSServerConfig {

    }

    public class DatabaseConfig {

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("database")]
        public string Database { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

    }
}
