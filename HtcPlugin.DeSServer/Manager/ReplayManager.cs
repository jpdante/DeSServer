using System.Collections.Generic;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using HtcPlugin.DeSServer.Model;
using MySqlConnector;

namespace HtcPlugin.DeSServer.Manager {
    public class ReplayManager {

        public ReplayManager() {
        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }

        public async Task<Replay[]> GetReplays(int blockId, int max) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT id, player_id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id FROM replays WHERE block_id = @blockId ORDER BY id DESC LIMIT @max;", conn);
            cmd.Parameters.AddWithValue("blockId", blockId);
            cmd.Parameters.AddWithValue("max", max);
            var replays = new List<Replay>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                replays.Add(new Replay {
                    Id = reader.GetInt32(0),
                    PlayerId = reader.GetString(1),
                    BlockId = reader.GetInt32(2),
                    PosX = reader.GetFloat(3),
                    PosY = reader.GetFloat(4),
                    PosZ = reader.GetFloat(5),
                    RotX = reader.GetFloat(6),
                    RotY = reader.GetFloat(7),
                    RotZ = reader.GetFloat(8),
                    MsgId = reader.GetInt32(9),
                    MainMsgId = reader.GetInt32(10),
                    MsgCateId = reader.GetInt32(11),
                });
            }
            return replays.ToArray();
        }

        public void AddWanderingGhost() {
        
        }
    }
}