using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace HtcPlugin.DeSServer.Core {
    public class DatabaseContext {

        private static string _connectionString;

        public DatabaseContext(DatabaseConfig databaseConfig) {
            _connectionString = $"Server={databaseConfig.Host};Port={databaseConfig.Port};Database={databaseConfig.Database};Uid={databaseConfig.Username};Pwd={databaseConfig.Password};";
        }

        public static async Task<MySqlConnection> GetConnection() {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync(); 
            return connection;
        }
    }
}
