using System.Collections.Generic;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using HtcPlugin.DeSServer.Model;
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

            return messages.ToArray();
        }
    }
}