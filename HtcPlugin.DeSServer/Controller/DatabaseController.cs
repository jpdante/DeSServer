using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Controller.DatabaseModels;
using HtcPlugin.DeSServer.Core;
using HtcSharp.HttpModule.Http.Abstractions;
using HtcSharp.HttpModule.Mvc;
using MySqlConnector;

namespace HtcPlugin.DeSServer.Controller {
    public class DatabaseController {

        public const string Version = "v1";

        [HttpPost("/api/" + Version + "/user")]
        public static async Task GetUser(HttpContext httpContext, GetUser user) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT grade_s, grade_a, grade_b, grade_c, grade_d, logins, sessions, msg_rating, tendency, desired_tendency, use_desired, play_time, creation_date FROM players WHERE player_id = @playerId;", conn);
            cmd.Parameters.AddWithValue("playerId", $"{user.Username}0");
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                int gradeS = reader.GetInt32(0);
                int gradeA = reader.GetInt32(1);
                int gradeB = reader.GetInt32(2);
                int gradeC = reader.GetInt32(3);
                int gradeD = reader.GetInt32(4);
                uint logins = reader.GetUInt32(5);
                uint sessions = reader.GetUInt32(6);
                int msgRating = reader.GetInt32(7);
                int tendency = reader.GetInt32(8);
                int desiredTendency = reader.GetInt32(9);
                bool useDesired = reader.GetBoolean(10);
                uint playTime = reader.GetUInt32(11);
                var creationDate = reader.GetDateTime(12);
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                    success = true,
                    player = new {
                        gradeS,
                        gradeA,
                        gradeB,
                        gradeC,
                        gradeD,
                        logins,
                        sessions,
                        msgRating,
                        tendency,
                        desiredTendency,
                        useDesired,
                        playTime,
                        creationDate = ((DateTimeOffset)creationDate).ToUnixTimeSeconds(),
                        creationDateString = creationDate,
                    }
                });
            } else {
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                    success = false,
                    error = new {
                        id = 404,
                        message = "User not found."
                    }
                });
            }
        }

        [HttpPost("/api/" + Version + "/tendency")]
        public static async Task SetTendency(HttpContext httpContext, SetTendency tendency) {
            await using var conn = await DatabaseContext.GetConnection();

            await using (var cmd = new MySqlCommand("SELECT creation_date FROM players WHERE player_id = @playerId;", conn)) {
                cmd.Parameters.AddWithValue("playerId", $"{tendency.Username}0");
                await using (var reader = await cmd.ExecuteReaderAsync()) {
                    if (!await reader.ReadAsync() || !reader.HasRows) {
                        await JsonSerializer.SerializeAsync(httpContext.Response.Body, new { success = false, error = new { id = 404, message = "User not found." } });
                        return;
                    }
                }
            }

            await using (var cmd = new MySqlCommand("UPDATE players SET desired_tendency = @desiredTendency, use_desired = @useDesired WHERE player_id = @playerId;", conn)) {
                cmd.Parameters.AddWithValue("playerId", $"{tendency.Username}0");
                cmd.Parameters.AddWithValue("desiredTendency", tendency.DesiredTendency);
                cmd.Parameters.AddWithValue("useDesired", tendency.UseDesired);
                await cmd.ExecuteNonQueryAsync();
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                    success = true
                });
                return;
            }
        }

        [HttpPost("/api/" + Version + "/message")]
        public static async Task GetMessage(HttpContext httpContext, GetMessage message) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, rating, creation_date FROM messages WHERE player_id = @playerId ORDER BY id DESC LIMIT 1;", conn);
            cmd.Parameters.AddWithValue("playerId", $"{message.Username}0");
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync() && reader.HasRows) {
                uint id = reader.GetUInt32(0);
                uint blockId = reader.GetUInt32(1);
                float posX = reader.GetFloat(2);
                float posY = reader.GetFloat(3);
                float posZ = reader.GetFloat(4);
                float rotX = reader.GetFloat(5);
                float rotY = reader.GetFloat(6);
                float rotZ = reader.GetFloat(7);
                int msgId = reader.GetInt32(8);
                int mainMsgId = reader.GetInt32(9);
                int msgCateId = reader.GetInt32(10);
                int rating = reader.GetInt32(11);
                var creationDate = reader.GetDateTime(12);
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                    success = true,
                    message = new {
                        id,
                        blockId,
                        posX,
                        posY,
                        posZ,
                        rotX,
                        rotY,
                        rotZ,
                        msgId,
                        mainMsgId,
                        msgCateId,
                        rating,
                        creationDate = ((DateTimeOffset)creationDate).ToUnixTimeSeconds(),
                        creationDateString = creationDate,
                    }
                });
            } else {
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                    success = false,
                    error = new {
                        id = 404,
                        message = "User not found."
                    }
                });
            }
        }

        [HttpPost("/api/" + Version + "/messages")]
        public static async Task GetMessages(HttpContext httpContext, GetMessages get) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, rating, creation_date FROM messages WHERE player_id = @playerId ORDER BY id DESC LIMIT @from,@to;", conn);
            cmd.Parameters.AddWithValue("playerId", $"{get.Username}0");
            cmd.Parameters.AddWithValue("from", get.From);
            cmd.Parameters.AddWithValue("to", get.To);
            await using var reader = await cmd.ExecuteReaderAsync();
            var messages = new List<object>();
            while (await reader.ReadAsync()) {
                uint id = reader.GetUInt32(0);
                uint blockId = reader.GetUInt32(1);
                float posX = reader.GetFloat(2);
                float posY = reader.GetFloat(3);
                float posZ = reader.GetFloat(4);
                float rotX = reader.GetFloat(5);
                float rotY = reader.GetFloat(6);
                float rotZ = reader.GetFloat(7);
                int msgId = reader.GetInt32(8);
                int mainMsgId = reader.GetInt32(9);
                int msgCateId = reader.GetInt32(10);
                int rating = reader.GetInt32(11);
                var creationDate = reader.GetDateTime(12);
                messages.Add(new {
                    id,
                    blockId,
                    posX,
                    posY,
                    posZ,
                    rotX,
                    rotY,
                    rotZ,
                    msgId,
                    mainMsgId,
                    msgCateId,
                    rating,
                    creationDate = ((DateTimeOffset)creationDate).ToUnixTimeSeconds(),
                    creationDateString = creationDate,
                });
            }
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                success = true,
                messages
            });
        }

        [HttpPost("/api/" + Version + "/replay")]
        public static async Task GetReplay(HttpContext httpContext, GetReplay replay) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, creation_date FROM replays WHERE player_id = @playerId ORDER BY id DESC LIMIT 1;", conn);
            cmd.Parameters.AddWithValue("playerId", $"{replay.Username}0");
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync() && reader.HasRows) {
                uint id = reader.GetUInt32(0);
                uint blockId = reader.GetUInt32(1);
                float posX = reader.GetFloat(2);
                float posY = reader.GetFloat(3);
                float posZ = reader.GetFloat(4);
                float rotX = reader.GetFloat(5);
                float rotY = reader.GetFloat(6);
                float rotZ = reader.GetFloat(7);
                int msgId = reader.GetInt32(8);
                int mainMsgId = reader.GetInt32(9);
                int msgCateId = reader.GetInt32(10);
                var creationDate = reader.GetDateTime(11);
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                    success = true,
                    replay = new {
                        id,
                        blockId,
                        posX,
                        posY,
                        posZ,
                        rotX,
                        rotY,
                        rotZ,
                        msgId,
                        mainMsgId,
                        msgCateId,
                        creationDate = ((DateTimeOffset)creationDate).ToUnixTimeSeconds(),
                        creationDateString = creationDate,
                    }
                });
            } else {
                await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                    success = false,
                    error = new {
                        id = 404,
                        message = "User not found."
                    }
                });
            }
        }

        [HttpPost("/api/" + Version + "/replays")]
        public static async Task GetReplays(HttpContext httpContext, GetReplays get) {
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("SELECT id, block_id, pos_x, pos_y, pos_z, rot_x, rot_y, rot_z, msg_id, main_msg_id, msg_cate_id, creation_date FROM replays WHERE player_id = @playerId ORDER BY id DESC LIMIT @from,@to;", conn);
            cmd.Parameters.AddWithValue("playerId", $"{get.Username}0");
            cmd.Parameters.AddWithValue("from", get.From);
            cmd.Parameters.AddWithValue("to", get.To);
            await using var reader = await cmd.ExecuteReaderAsync();
            var replays = new List<object>();
            while (await reader.ReadAsync()) {
                uint id = reader.GetUInt32(0);
                uint blockId = reader.GetUInt32(1);
                float posX = reader.GetFloat(2);
                float posY = reader.GetFloat(3);
                float posZ = reader.GetFloat(4);
                float rotX = reader.GetFloat(5);
                float rotY = reader.GetFloat(6);
                float rotZ = reader.GetFloat(7);
                int msgId = reader.GetInt32(8);
                int mainMsgId = reader.GetInt32(9);
                int msgCateId = reader.GetInt32(10);
                var creationDate = reader.GetDateTime(11);
                replays.Add(new {
                    id,
                    blockId,
                    posX,
                    posY,
                    posZ,
                    rotX,
                    rotY,
                    rotZ,
                    msgId,
                    mainMsgId,
                    msgCateId,
                    creationDate = ((DateTimeOffset)creationDate).ToUnixTimeSeconds(),
                    creationDateString = creationDate,
                });
            }
            await JsonSerializer.SerializeAsync(httpContext.Response.Body, new {
                success = true,
                replays
            });
        }
    }
}