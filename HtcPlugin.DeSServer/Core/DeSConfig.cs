using System.Collections.Generic;
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
                Host = "127.0.0.1",
                Port = 18000,
                ReturnLocalhostOnLocal = true,
                Interval = 120,
                GetWanderingGhostInterval = 20,
                SetWanderingGhostInterval = 20,
                GetBloodMessageNum = 80,
                GetReplayListNum = 80,
                EnableWanderingGhost = true,
                Motd = new List<string> {
                    "Hey, welcome to my DeS Server!\r\nHave fun!"
                },
                PlayerHeartbeatTimeout = 60, //  1 Minute
                SessionHeartbeatTimeout = 5 * 60, // 5 Minutes
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

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("returnLocalhostOnLocal")]
        public bool ReturnLocalhostOnLocal { get; set; }

        [JsonPropertyName("interval")]
        public int Interval { get; set; }

        [JsonPropertyName("getWanderingGhostInterval")]
        public int GetWanderingGhostInterval { get; set; }

        [JsonPropertyName("setWanderingGhostInterval")]
        public int SetWanderingGhostInterval { get; set; }

        [JsonPropertyName("getBloodMessageNum")]
        public int GetBloodMessageNum { get; set; }

        [JsonPropertyName("getReplayListNum")]
        public int GetReplayListNum { get; set; }

        [JsonPropertyName("enableWanderingGhost")]
        public bool EnableWanderingGhost { get; set; }

        [JsonPropertyName("motd")]
        public List<string> Motd { get; set; }

        [JsonPropertyName("playerHeartbeatTimeout")]
        public int PlayerHeartbeatTimeout { get; set; }

        [JsonPropertyName("sessionHeartbeatTimeout")]
        public int SessionHeartbeatTimeout { get; set; }
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
