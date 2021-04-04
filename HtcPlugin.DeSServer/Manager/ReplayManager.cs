using System;
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

        public async Task<Replay[]> GetReplays(uint blockId, int max) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT id, player_id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id FROM replays WHERE block_id = @blockId ORDER BY id DESC LIMIT @max;", conn);
            cmd.Parameters.AddWithValue("blockId", blockId);
            cmd.Parameters.AddWithValue("max", max);
            var replays = new List<Replay>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                replays.Add(new Replay {
                    Id = reader.GetUInt32(0),
                    PlayerId = reader.GetString(1),
                    BlockId = reader.GetUInt32(2),
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

        public async Task<Replay> GetReplay(uint id) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT replay_data FROM replays WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("id", id);
            var replay = new List<Replay>();
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync() || !reader.HasRows) return null;
            return new Replay {
                ReplayData = Convert.FromBase64String(reader.GetString(0))
            };
        }

        public async Task AddReplay(Replay replay) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("INSERT INTO replays (player_id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, replay_data) VALUES (@playerId, @blockId, @posX, @posY, @posZ, @rotX, @rotY, @rotZ, @msgId, @mainMsgId, @msgCateId, @replayData);", conn);
            cmd.Parameters.AddWithValue("playerId", replay.PlayerId);
            cmd.Parameters.AddWithValue("blockId", replay.BlockId);
            cmd.Parameters.AddWithValue("posX", replay.PosX);
            cmd.Parameters.AddWithValue("posY", replay.PosY);
            cmd.Parameters.AddWithValue("posZ", replay.PosZ);
            cmd.Parameters.AddWithValue("rotX", replay.RotX);
            cmd.Parameters.AddWithValue("rotY", replay.RotX);
            cmd.Parameters.AddWithValue("rotZ", replay.RotX);
            cmd.Parameters.AddWithValue("msgId", replay.MsgId);
            cmd.Parameters.AddWithValue("mainMsgId", replay.MainMsgId);
            cmd.Parameters.AddWithValue("msgCateId", replay.MsgCateId);
            cmd.Parameters.AddWithValue("replayData", Convert.ToBase64String(replay.ReplayData));
            await cmd.ExecuteNonQueryAsync();
        }
    }
}