using System.Collections.Generic;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using HtcPlugin.DeSServer.Model;
using HtcSharp.Logging;
using MySqlConnector;

namespace HtcPlugin.DeSServer.Manager {
    public class MessageManager {

        public MessageManager() {

        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }

        public async Task<Message[]> GetMessages(string playerId, uint blockId, int max) {
            await using var conn = await DatabaseContext.GetConnection();
            var messages = new List<Message>();

            await using (var cmd = new MySqlCommand("SELECT id, player_id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, rating FROM messages WHERE player_id = @playerId AND block_id = @blockId ORDER BY id DESC LIMIT @max;", conn)) {
                cmd.Parameters.AddWithValue("playerId", playerId);
                cmd.Parameters.AddWithValue("blockId", blockId);
                cmd.Parameters.AddWithValue("max", max - messages.Count);
                await using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        messages.Add(new Message {
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
                            Rating = reader.GetInt32(12),
                        });
                    }
                }
            }

            await using (var cmd = new MySqlCommand("SELECT id, player_id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, rating FROM messages WHERE player_id != @playerId AND block_id = @blockId ORDER BY id DESC LIMIT @max;", conn)) {
                cmd.Parameters.AddWithValue("playerId", playerId);
                cmd.Parameters.AddWithValue("blockId", blockId);
                cmd.Parameters.AddWithValue("max", max - messages.Count);
                await using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        messages.Add(new Message {
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
                            Rating = reader.GetInt32(12),
                        });
                    }
                }
            }

            HtcPlugin.Logger.LogInfo($"[MessageManager] Get blood message list");
            return messages.ToArray();
        }

        public async Task AddMessage(Message message) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("INSERT INTO messages (player_id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, rating) VALUES (@playerId, @blockId, @posX, @posY, @posZ, @rotX, @rotY, @rotZ, @msgId, @mainMsgId, @msgCateId, @rating);", conn);
            cmd.Parameters.AddWithValue("playerId", message.PlayerId);
            cmd.Parameters.AddWithValue("blockId", message.BlockId);
            cmd.Parameters.AddWithValue("posX", message.PosX);
            cmd.Parameters.AddWithValue("posY", message.PosY);
            cmd.Parameters.AddWithValue("posZ",message.PosZ);
            cmd.Parameters.AddWithValue("rotX", message.RotX);
            cmd.Parameters.AddWithValue("rotY", message.RotY);
            cmd.Parameters.AddWithValue("rotZ", message.RotZ);
            cmd.Parameters.AddWithValue("msgId", message.MsgId);
            cmd.Parameters.AddWithValue("mainMsgId", message.MainMsgId);
            cmd.Parameters.AddWithValue("msgCateId", message.MsgCateId);
            cmd.Parameters.AddWithValue("rating", 0);
            await cmd.ExecuteNonQueryAsync();
            HtcPlugin.Logger.LogInfo($"[MessageManager] Add {message.PlayerId} blood message");
        }

        public async Task DeleteMessage(uint id) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("DELETE FROM messages WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
            HtcPlugin.Logger.LogInfo($"[MessageManager] Delete blood message ID: {id}");
        }

        public async Task RecommendMessage(uint id) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("UPDATE messages set rating = rating + 1 WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            await cmd.ExecuteNonQueryAsync();
            HtcPlugin.Logger.LogInfo($"[MessageManager] Recommended blood message ID: {id}");
        }
    }
}