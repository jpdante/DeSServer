using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Model;
using HtcSharp.Core.Logging.Abstractions;
using HtcSharp.HttpModule.Http.Abstractions;
using HtcSharp.HttpModule.Http.Abstractions.Extensions;
using HtcSharp.HttpModule.Mvc;
using Microsoft.Extensions.Primitives;
// ReSharper disable InconsistentNaming

namespace HtcPlugin.DeSServer.Controller {
    public class DeSController {

        private static List<string> ProcessList = new List<string>();

        private static async Task<string> PrepareResponse(HttpContext httpContext, byte cmd, byte[] data) {
            var memoryStream = new MemoryStream();
            memoryStream.WriteByte(cmd);
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint)data.Length + 5));
            await memoryStream.WriteAsync(data);
            return Convert.ToBase64String(memoryStream.ToArray()) + "\n";
        }

        private static async Task SendResponse(HttpContext httpContext, string data) {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.Headers.Add("Date", new StringValues(DateTime.Now.ToString("r")));
            httpContext.Response.Headers.Add("Content-Length", data.Length.ToString());
            httpContext.Response.Headers.Add("Connection", "close");
            httpContext.Response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
            await httpContext.Response.WriteAsync(data);
            /*HtcPlugin.Logger.LogInfo($"{httpContext.Connection.Id} <= {{");
            HtcPlugin.Logger.LogInfo($"    Headers {{");
            foreach (KeyValuePair<string, StringValues> headers in httpContext.Response.Headers) {
                HtcPlugin.Logger.LogInfo($"        {headers.Key}: \"{headers.Value}\"");
            }
            HtcPlugin.Logger.LogInfo($"    }}");
            HtcPlugin.Logger.LogInfo($"    Body: {data.Remove(data.Length - 1, 1)}");
            HtcPlugin.Logger.LogInfo($"}}");*/
            ProcessList.Remove(httpContext.Connection.Id);
            foreach (string conn in ProcessList) {
                HtcPlugin.Logger.LogWarn($"Response for {conn} was not send!");
            }
        }

        private static async Task<string> GetAndDecryptData(HttpContext httpContext) {
            if (!httpContext.Request.Headers.TryGetValue("Content-Length", out var contentLengthRaw)) throw new HttpException(500, "Missing Content-Length.");
            if (!int.TryParse(contentLengthRaw, out int contentLength)) throw new HttpException(500, "Failed to convert Content-Length to integer.");
            if (contentLength > 10000000) throw new HttpException(500, "Request is too big.");
            if (contentLength < 17) throw new HttpException(500, "Request is too small.");

            try {
                byte[] aesKey = { 49, 49, 49, 49, 49, 49, 49, 49, 50, 50, 50, 50, 50, 50, 50, 50, 51, 51, 51, 51, 51, 51, 51, 51, 52, 52, 52, 52, 52, 52, 52, 52 };
                var aesIv = new byte[16];
                await httpContext.Request.Body.ReadAsync(aesIv, 0, aesIv.Length);

                var encryptedData = new byte[contentLength - 16];
                await httpContext.Request.Body.ReadAsync(encryptedData, 0, encryptedData.Length);
                await using var encryptedStream = new MemoryStream(encryptedData);

                using var aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                var cryptoTransform = aes.CreateDecryptor(aesKey, aesIv);

                await using var cryptoStream = new CryptoStream(encryptedStream, cryptoTransform, CryptoStreamMode.Read);
                using var streamReader = new StreamReader(cryptoStream);

                return await streamReader.ReadToEndAsync();
            } catch (Exception ex) {
                HtcPlugin.Logger.LogError(ex.Message);
                HtcPlugin.Logger.LogError(ex.StackTrace);
            }
            return null;
        }

        private static byte[] DecodeBrokenBase64(string data) {
            var fixedData = new StringBuilder();
            const string okChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/+";
            foreach (char c in data) {
                if (okChars.Contains(c)) fixedData.Append(c);
                else if (c == ' ') fixedData.Append('+');
            }
            switch (fixedData.Length % 4) {
                case 3:
                    fixedData.Append('=');
                    break;
                case 2:
                    fixedData.Append("==");
                    break;
                case 1:
                    fixedData.Append("A==");
                    break;
            }
            return Convert.FromBase64String(fixedData.ToString());
        }

        private static Dictionary<string, string> ParamData(string data) {
            var parameters = new Dictionary<string, string>();
            foreach (string param in data.Split("&")) {
                if (param == "\x00" || param == "") continue;
                if (!param.Contains("=")) continue;
                string[] keyValue = param.Split("=", 2);
                parameters.Add(keyValue[0], keyValue[1]);
            }
            return parameters;
        }

        private static void PrintRequest(HttpContext httpContext, string data = null) {
            ProcessList.Add(httpContext.Connection.Id);
            /*HtcPlugin.Logger.LogInfo($"{httpContext.Connection.Id} => {httpContext.Request.Method} {httpContext.Request.Path} {{");
            HtcPlugin.Logger.LogInfo($"    Host: {httpContext.Request.Host}");
            HtcPlugin.Logger.LogInfo($"    ContentType: {httpContext.Request.ContentType}");
            HtcPlugin.Logger.LogInfo($"    Query: {httpContext.Request.QueryString}");
            HtcPlugin.Logger.LogInfo($"    Header: {{");
            foreach ((string key, var value) in httpContext.Request.Headers) {
                HtcPlugin.Logger.LogInfo($"        {key}: \"{value}\"");
            }
            HtcPlugin.Logger.LogInfo($"    }},");
            if (data != null) HtcPlugin.Logger.LogInfo($"    Body: {data}");
            HtcPlugin.Logger.LogInfo($"}}");*/
        }

        [HttpPost("/demons-souls-us/ss.info")]
        public static async Task SsInfo(HttpContext httpContext) {
            httpContext.Response.StatusCode = 200;
            string host;
            if (HtcPlugin.Config.DeSServer.ReturnLocalhostOnLocal && httpContext.Connection.RemoteIpAddress.ToString().Equals("127.0.0.1")) {
                host = $"127.0.0.1:{HtcPlugin.Config.DeSServer.Port}";
            } else {
                host = $"{HtcPlugin.Config.DeSServer.Host}:{HtcPlugin.Config.DeSServer.Port}";
            }
            await httpContext.Response.WriteAsync(
                "<ss>0</ss>\r\n" +
                "<lang1></lang1>\r\n" +
                "<lang2></lang2>\r\n" +
                "<lang3></lang3>\r\n" +
                "<lang4></lang4>\r\n" +
                "<lang5></lang5>\r\n" +
                "<lang6></lang6>\r\n" +
                "<lang7></lang7>\r\n" +
                "<lang8></lang8>\r\n" +
                "<lang11></lang11>\r\n" +
                "<lang12></lang12>\r\n" +
                $"<gameurl1>http://{host}/cgi-bin/</gameurl1>\r\n" +
                $"<gameurl2>http://{host}/cgi-bin/</gameurl2>\r\n" +
                $"<gameurl3>http://{host}/cgi-bin/</gameurl3>\r\n" +
                $"<gameurl4>http://{host}/cgi-bin/</gameurl4>\r\n" +
                $"<gameurl5>http://{host}/cgi-bin/</gameurl5>\r\n" +
                $"<gameurl6>http://{host}/cgi-bin/</gameurl6>\r\n" +
                $"<gameurl7>http://{host}/cgi-bin/</gameurl7>\r\n" +
                $"<gameurl8>http://{host}/cgi-bin/</gameurl8>\r\n" +
                $"<gameurl11>http://{host}/cgi-bin/</gameurl11>\r\n" +
                $"<gameurl12>http://{host}/cgi-bin/</gameurl12>\r\n" +
                "<browserurl1></browserurl1>\r\n" +
                "<browserurl2></browserurl2>\r\n" +
                "<browserurl3></browserurl3>\r\n" +
                $"<interval1>{HtcPlugin.Config.DeSServer.Interval}</interval1>\r\n" +
                $"<interval2>{HtcPlugin.Config.DeSServer.Interval}</interval2>\r\n" +
                $"<interval3>{HtcPlugin.Config.DeSServer.Interval}</interval3>\r\n" +
                $"<interval4>{HtcPlugin.Config.DeSServer.Interval}</interval4>\r\n" +
                $"<interval5>{HtcPlugin.Config.DeSServer.Interval}</interval5>\r\n" +
                $"<interval6>{HtcPlugin.Config.DeSServer.Interval}</interval6>\r\n" +
                $"<interval7>{HtcPlugin.Config.DeSServer.Interval}</interval7>\r\n" +
                $"<interval8>{HtcPlugin.Config.DeSServer.Interval}</interval8>\r\n" +
                $"<interval11>{HtcPlugin.Config.DeSServer.Interval}</interval11>\r\n" +
                $"<interval12>{HtcPlugin.Config.DeSServer.Interval}</interval12>\r\n" +
                $"<getWanderingGhostInterval>{HtcPlugin.Config.DeSServer.GetWanderingGhostInterval}</getWanderingGhostInterval>\r\n" +
                $"<setWanderingGhostInterval>{HtcPlugin.Config.DeSServer.SetWanderingGhostInterval}</setWanderingGhostInterval>\r\n" +
                $"<getBloodMessageNum>{HtcPlugin.Config.DeSServer.GetBloodMessageNum}</getBloodMessageNum>\r\n" +
                $"<getReplayListNum>{HtcPlugin.Config.DeSServer.GetReplayListNum}</getReplayListNum>\r\n" +
                $"<enableWanderingGhost>{(HtcPlugin.Config.DeSServer.EnableWanderingGhost ? 1 : 0)}</enableWanderingGhost>"); 
        }

        [HttpPost("/cgi-bin/login.spd")]
        public static async Task Login(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            const string motd = "DeS test server\r\n";
            const string motd2 = "Furries are wonderful\r\n";
            string responseData = await PrepareResponse(httpContext, 0x02, Encoding.ASCII.GetBytes("\x01\x02" + motd + "\x00" + motd2 + "\x00"));
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/initializeCharacter.spd")]
        public static async Task InitializeCharacter(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string characterId)) throw new HttpException(500, "Missing character id.");
            if (!data.TryGetValue("index", out string index)) throw new HttpException(500, "Missing index.");
            
            string response = await HtcPlugin.Server.PlayerManager.InitPlayer(httpContext.Connection.RemoteIpAddress.ToString(), characterId, index);
            string responseData = await PrepareResponse(httpContext, 0x17, Encoding.ASCII.GetBytes(response));
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getQWCData.spd")]
        public static async Task GetQWCData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!HtcPlugin.Server.PlayerManager.GetPlayerByHost(httpContext.Connection.RemoteIpAddress.ToString(), out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            short tendency = await player.GetWorldTendency();
            await using var memoryStream = new MemoryStream();
            for (var i = 0; i < 7; i++) {
                await memoryStream.WriteAsync(BitConverter.GetBytes(tendency));
                await memoryStream.WriteAsync(BitConverter.GetBytes((short)0));
            }
            string responseData = await PrepareResponse(httpContext, 0x0e, memoryStream.ToArray());
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/addQWCData.spd")]
        public static async Task AddQWCData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            string responseData = await PrepareResponse(httpContext, 0x09, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getMultiPlayGrade.spd")]
        public static async Task GetMultiPlayGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("NPID", out string NPID)) throw new HttpException(500, "Missing NPID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(NPID, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            int[] grades = await player.GetMultiPlayGrade();
            await using var memoryStream = new MemoryStream();
            memoryStream.WriteByte(0x01);
            foreach (int grade in grades) {
                await memoryStream.WriteAsync(BitConverter.GetBytes(grade));
            }
            string responseData = await PrepareResponse(httpContext, 0x28, memoryStream.ToArray());
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getBloodMessageGrade.spd")]
        public static async Task GetBloodMessageGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("NPID", out string NPID)) throw new HttpException(500, "Missing NPID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(NPID, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            int rating = await player.GetBloodMessageGrade();
            await using var memoryStream = new MemoryStream();
            memoryStream.WriteByte(0x01);
            await memoryStream.WriteAsync(BitConverter.GetBytes(rating));
            string responseData = await PrepareResponse(httpContext, 0x29, memoryStream.ToArray());
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getTimeMessage.spd")]
        public static async Task GetTimeMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("NPID", out string NPID)) throw new HttpException(500, "Missing NPID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(NPID, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");
            
            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            string responseData = await PrepareResponse(httpContext, 0x22, Encoding.ASCII.GetBytes("\x00\x00\x00"));
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getAgreement.spd")]
        public static async Task GetAgreement(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            string responseData = await PrepareResponse(httpContext, 0x01, Encoding.ASCII.GetBytes("\x01\x01Unknown response.\r\n\x00"));
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/addNewAccount.spd")]
        public static async Task AddNewAccount(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            string responseData = await PrepareResponse(httpContext, 0x01, Encoding.ASCII.GetBytes("\x01\x01Unknown response.\r\n\x00"));
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getBloodMessage.spd")]
        public static async Task GetBloodMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!data.TryGetValue("blockID", out string blockIdRaw)) throw new HttpException(500, "Missing blockID.");
            if (!data.TryGetValue("replayNum", out string messageNumRaw)) throw new HttpException(500, "Missing replayNum.");
            
            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!int.TryParse(messageNumRaw, out int messageNum)) throw new HttpException(500, "Failed to parse replayNum.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            Message[] messages = await HtcPlugin.Server.MessageManager.GetMessages(playerId, blockId, messageNum);
            await using var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint)messages.Length));
            foreach (var message in messages) {
                await memoryStream.WriteAsync(await message.GenerateHeader());
            }
            string responseData = await PrepareResponse(httpContext, 0x1f, memoryStream.ToArray());
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/addBloodMessage.spd")]
        public static async Task AddBloodMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!data.TryGetValue("blockID", out string blockIdRaw)) throw new HttpException(500, "Missing blockID.");
            if (!data.TryGetValue("posx", out string posXRaw)) throw new HttpException(500, "Missing posx.");
            if (!data.TryGetValue("posy", out string posYRaw)) throw new HttpException(500, "Missing posy.");
            if (!data.TryGetValue("posz", out string posZRaw)) throw new HttpException(500, "Missing posz.");
            if (!data.TryGetValue("angx", out string rotXRaw)) throw new HttpException(500, "Missing angx.");
            if (!data.TryGetValue("angy", out string rotYRaw)) throw new HttpException(500, "Missing angy.");
            if (!data.TryGetValue("angz", out string rotZRaw)) throw new HttpException(500, "Missing angz.");
            if (!data.TryGetValue("messageID", out string messageIdRaw)) throw new HttpException(500, "Missing messageID.");
            if (!data.TryGetValue("mainMsgID", out string mainMsgIdRaw)) throw new HttpException(500, "Missing mainMsgID.");
            if (!data.TryGetValue("addMsgCateID", out string addMsgCateIdRaw)) throw new HttpException(500, "Missing addMsgCateID.");

            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!float.TryParse(posXRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posX)) throw new HttpException(500, "Failed to parse posx.");
            if (!float.TryParse(posYRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posY)) throw new HttpException(500, "Failed to parse posy.");
            if (!float.TryParse(posZRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posZ)) throw new HttpException(500, "Failed to parse posz.");
            if (!float.TryParse(rotXRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotX)) throw new HttpException(500, "Failed to parse angx.");
            if (!float.TryParse(rotYRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotY)) throw new HttpException(500, "Failed to parse angy.");
            if (!float.TryParse(rotZRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotZ)) throw new HttpException(500, "Failed to parse angz.");
            if (!int.TryParse(messageIdRaw, out int messageId)) throw new HttpException(500, "Failed to parse messageID.");
            if (!int.TryParse(mainMsgIdRaw, out int mainMsgId)) throw new HttpException(500, "Failed to parse mainMsgID.");
            if (!int.TryParse(addMsgCateIdRaw, out int addMsgCateId)) throw new HttpException(500, "Failed to parse messageID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            var message = new Message {
                PlayerId = playerId,
                BlockId = blockId,
                PosX = posX,
                PosY = posY,
                PosZ = posZ,
                RotX = rotX,
                RotY = rotY,
                RotZ = rotZ,
                MsgId = messageId,
                MainMsgId = mainMsgId,
                MsgCateId = addMsgCateId,
                Rating = 0
            };

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            await HtcPlugin.Server.MessageManager.AddMessage(message);
            string responseData = await PrepareResponse(httpContext, 0x1d, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/updateBloodMessageGrade.spd")]
        public static async Task UpdateBloodMessageGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("bmID", out string bmIdRaw)) throw new HttpException(500, "Missing bmID.");
            if (!uint.TryParse(bmIdRaw, out uint bmId)) throw new HttpException(500, "Failed to parse bmID.");

            await HtcPlugin.Server.MessageManager.RecommendMessage(bmId);
            string responseData = await PrepareResponse(httpContext, 0x27, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/deleteBloodMessage.spd")]
        public static async Task DeleteBloodMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("bmID", out string bmIdRaw)) throw new HttpException(500, "Missing bmID.");
            if (!uint.TryParse(bmIdRaw, out uint bmId)) throw new HttpException(500, "Failed to parse bmID.");

            await HtcPlugin.Server.MessageManager.DeleteMessage(bmId);
            string responseData = await PrepareResponse(httpContext, 0x2a, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getReplayList.spd")]
        public static async Task GetReplayList(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("blockID", out string blockIdRaw)) throw new HttpException(500, "Missing blockID.");
            if (!data.TryGetValue("replayNum", out string replayNumRaw)) throw new HttpException(500, "Missing replayNum.");
            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!int.TryParse(replayNumRaw, out int replayNum)) throw new HttpException(500, "Failed to parse replayNum.");

            Replay[] replays = await HtcPlugin.Server.ReplayManager.GetReplays(blockId, replayNum);
            await using var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint)replays.Length));
            foreach (var replay in replays) {
                await memoryStream.WriteAsync(await replay.GenerateHeader());
            }
            string responseData = await PrepareResponse(httpContext, 0x1f, memoryStream.ToArray());
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getReplayData.spd")]
        public static async Task GetReplayData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("ghostID", out string ghostIDRaw)) throw new HttpException(500, "Missing ghostID.");
            if (!uint.TryParse(ghostIDRaw, out uint ghostID)) throw new HttpException(500, "Failed to parse ghostID.");

            var replay = await HtcPlugin.Server.ReplayManager.GetReplay(ghostID);
            await using var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(BitConverter.GetBytes(ghostID));
            if (replay != null) {
                await memoryStream.WriteAsync(BitConverter.GetBytes((uint) replay.ReplayData.Length));
                await memoryStream.WriteAsync(replay.ReplayData);
            } else {
                await memoryStream.WriteAsync(BitConverter.GetBytes(0));
            }
            string responseData = await PrepareResponse(httpContext, 0x1e, memoryStream.ToArray());
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/addReplayData.spd")]
        public static async Task AddReplayData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!data.TryGetValue("blockID", out string blockIdRaw)) throw new HttpException(500, "Missing blockID.");
            if (!data.TryGetValue("posx", out string posXRaw)) throw new HttpException(500, "Missing posx.");
            if (!data.TryGetValue("posy", out string posYRaw)) throw new HttpException(500, "Missing posy.");
            if (!data.TryGetValue("posz", out string posZRaw)) throw new HttpException(500, "Missing posz.");
            if (!data.TryGetValue("angx", out string rotXRaw)) throw new HttpException(500, "Missing angx.");
            if (!data.TryGetValue("angy", out string rotYRaw)) throw new HttpException(500, "Missing angy.");
            if (!data.TryGetValue("angz", out string rotZRaw)) throw new HttpException(500, "Missing angz.");
            if (!data.TryGetValue("messageID", out string messageIdRaw)) throw new HttpException(500, "Missing messageID.");
            if (!data.TryGetValue("mainMsgID", out string mainMsgIdRaw)) throw new HttpException(500, "Missing mainMsgID.");
            if (!data.TryGetValue("addMsgCateID", out string addMsgCateIdRaw)) throw new HttpException(500, "Missing addMsgCateID.");
            if (!data.TryGetValue("replayBinary", out string replayDataRaw)) throw new HttpException(500, "Missing replayBinary.");

            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!float.TryParse(posXRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posX)) throw new HttpException(500, "Failed to parse posx.");
            if (!float.TryParse(posYRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posY)) throw new HttpException(500, "Failed to parse posy.");
            if (!float.TryParse(posZRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posZ)) throw new HttpException(500, "Failed to parse posz.");
            if (!float.TryParse(rotXRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotX)) throw new HttpException(500, "Failed to parse angx.");
            if (!float.TryParse(rotYRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotY)) throw new HttpException(500, "Failed to parse angy.");
            if (!float.TryParse(rotZRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotZ)) throw new HttpException(500, "Failed to parse angz.");
            if (!int.TryParse(messageIdRaw, out int messageId)) throw new HttpException(500, "Failed to parse messageID.");
            if (!int.TryParse(mainMsgIdRaw, out int mainMsgId)) throw new HttpException(500, "Failed to parse mainMsgID.");
            if (!int.TryParse(addMsgCateIdRaw, out int addMsgCateId)) throw new HttpException(500, "Failed to parse messageID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            byte[] replayData = DecodeBrokenBase64(replayDataRaw);
            var replay = new Replay {
                PlayerId = playerId,
                BlockId = blockId,
                PosX = posX,
                PosY = posY,
                PosZ = posZ,
                RotX = rotX,
                RotY = rotY,
                RotZ = rotZ,
                MsgId = messageId,
                MainMsgId = mainMsgId,
                MsgCateId = addMsgCateId,
                ReplayData = replayData
            };
            await HtcPlugin.Server.ReplayManager.AddReplay(replay);
            string responseData = await PrepareResponse(httpContext, 0x1d, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getWanderingGhost.spd")]
        public static async Task GetWanderingGhost(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!data.TryGetValue("blockID", out string blockIdRaw)) throw new HttpException(500, "Missing blockID.");
            if (!data.TryGetValue("maxGhostNum", out string maxGhostNumRaw)) throw new HttpException(500, "Missing maxGhostNum.");

            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!int.TryParse(maxGhostNumRaw, out int maxGhostNum)) throw new HttpException(500, "Failed to parse maxGhostNum.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            Ghost[] ghosts = HtcPlugin.Server.GhostManager.GetWanderingGhosts(playerId, blockId, maxGhostNum);
            await using var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint)0));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint)ghosts.Length));
            foreach (var ghost in ghosts) {
                string replayData64 = Convert.ToBase64String(ghost.ReplayData);
                byte[] replayData = Encoding.ASCII.GetBytes(replayData64);
                await memoryStream.WriteAsync(BitConverter.GetBytes((uint)replayData.Length));
                await memoryStream.WriteAsync(replayData);
            }
            string responseData = await PrepareResponse(httpContext, 0x11, memoryStream.ToArray());
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/setWanderingGhost.spd")]
        public static async Task SetWanderingGhost(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!data.TryGetValue("ghostBlockID", out string blockIdRaw)) throw new HttpException(500, "Missing ghostBlockID.");
            if (!data.TryGetValue("replayData", out string replayDataRaw)) throw new HttpException(500, "Missing replayData.");

            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            byte[] replayData = DecodeBrokenBase64(replayDataRaw);
            HtcPlugin.Server.GhostManager.AddWanderingGhost(new Ghost(playerId, blockId, replayData));
            string responseData = await PrepareResponse(httpContext, 0x17, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/getSosData.spd")]
        public static async Task GetSosData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("blockID", out string blockIdRaw)) throw new HttpException(500, "Missing blockID.");
            if (!data.TryGetValue("sosNum", out string sosNumRaw)) throw new HttpException(500, "Missing sosNum.");
            if (!data.TryGetValue("sosList", out string sosListRaw)) throw new HttpException(500, "Missing sosList.");

            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!uint.TryParse(sosNumRaw, out uint sosNum)) throw new HttpException(500, "Failed to parse sosNum.");
            string[] sosList = sosListRaw.Split("a0a");

            byte[] sessionData = await HtcPlugin.Server.SessionManager.GetSessionData(blockId, sosNum, sosList);
            string responseData = await PrepareResponse(httpContext, 0x0f, sessionData);
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/addSosData.spd")]
        public static async Task AddSosData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!data.TryGetValue("blockID", out string blockIdRaw)) throw new HttpException(500, "Missing blockID.");
            if (!data.TryGetValue("posx", out string posXRaw)) throw new HttpException(500, "Missing posx.");
            if (!data.TryGetValue("posy", out string posYRaw)) throw new HttpException(500, "Missing posy.");
            if (!data.TryGetValue("posz", out string posZRaw)) throw new HttpException(500, "Missing posz.");
            if (!data.TryGetValue("angx", out string rotXRaw)) throw new HttpException(500, "Missing angx.");
            if (!data.TryGetValue("angy", out string rotYRaw)) throw new HttpException(500, "Missing angy.");
            if (!data.TryGetValue("angz", out string rotZRaw)) throw new HttpException(500, "Missing angz.");
            if (!data.TryGetValue("messageID", out string messageIdRaw)) throw new HttpException(500, "Missing messageID.");
            if (!data.TryGetValue("mainMsgID", out string mainMsgIdRaw)) throw new HttpException(500, "Missing mainMsgID.");
            if (!data.TryGetValue("addMsgCateID", out string addMsgCateIdRaw)) throw new HttpException(500, "Missing addMsgCateID.");
            if (!data.TryGetValue("playerInfo", out string clientPlayerInfo)) throw new HttpException(500, "Missing playerInfo.");
            if (!data.TryGetValue("qwcwb", out string qwcwbRaw)) throw new HttpException(500, "Missing qwcwb.");
            if (!data.TryGetValue("qwclr", out string qwclrRaw)) throw new HttpException(500, "Missing qwclr.");
            if (!data.TryGetValue("isBlack", out string isBlackRaw)) throw new HttpException(500, "Missing isBlack.");
            if (!data.TryGetValue("playerLevel", out string playerLevelRaw)) throw new HttpException(500, "Missing playerLevel.");

            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");
            if (!uint.TryParse(blockIdRaw, out uint blockId)) throw new HttpException(500, "Failed to parse blockID.");
            if (!float.TryParse(posXRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posX)) throw new HttpException(500, "Failed to parse posx.");
            if (!float.TryParse(posYRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posY)) throw new HttpException(500, "Failed to parse posy.");
            if (!float.TryParse(posZRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float posZ)) throw new HttpException(500, "Failed to parse posz.");
            if (!float.TryParse(rotXRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotX)) throw new HttpException(500, "Failed to parse angx.");
            if (!float.TryParse(rotYRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotY)) throw new HttpException(500, "Failed to parse angy.");
            if (!float.TryParse(rotZRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotZ)) throw new HttpException(500, "Failed to parse angz.");
            if (!int.TryParse(messageIdRaw, out int messageId)) throw new HttpException(500, "Failed to parse messageID.");
            if (!int.TryParse(mainMsgIdRaw, out int mainMsgId)) throw new HttpException(500, "Failed to parse mainMsgID.");
            if (!int.TryParse(addMsgCateIdRaw, out int addMsgCateId)) throw new HttpException(500, "Failed to parse messageID.");
            if (!int.TryParse(qwcwbRaw, out int qwcwb)) throw new HttpException(500, "Failed to parse qwcwb.");
            if (!int.TryParse(qwclrRaw, out int qwclr)) throw new HttpException(500, "Failed to parse qwclr.");
            if (!int.TryParse(isBlackRaw, out int isBlack)) throw new HttpException(500, "Failed to parse isBlack.");
            if (!int.TryParse(playerLevelRaw, out int playerLevel)) throw new HttpException(500, "Failed to parse qwcwb.");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            bool success = await HtcPlugin.Server.SessionManager.CreateSession(player, blockId, posX, posY, posZ, rotX, rotY, rotZ, messageId, mainMsgId, addMsgCateId, clientPlayerInfo, qwcwb, qwclr, isBlack > 0, playerLevel);

            string responseData = await PrepareResponse(httpContext, 0x0a, new[] { success ? (byte)'\x01' : (byte)'\x00' }); // Test if sending 0x00 when fail to cancel the request.
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/checkSosData.spd")]
        public static async Task CheckSosData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            HtcPlugin.Server.SessionManager.Heartbeat(player);
            byte[] sessionData = HtcPlugin.Server.SessionManager.CheckSession(player);

            string responseData = await PrepareResponse(httpContext, 0x0b, sessionData);
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/outOfBlock.spd")]
        public static async Task OutOfBlock(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            HtcPlugin.Server.SessionManager.SetOutOfBlock(player);
            string responseData = await PrepareResponse(httpContext, 0x15, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/summonOtherCharacter.spd")]
        public static async Task SummonOtherCharacter(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("NPRoomID", out string NPRoomID)) throw new HttpException(500, "Missing NPRoomID.");
            if (!data.TryGetValue("ghostID", out string ghostIDRaw)) throw new HttpException(500, "Missing ghostID.");

            if (!uint.TryParse(ghostIDRaw, out uint ghostID)) throw new HttpException(500, "Failed to parse ghostID.");

            byte[] sessionData = HtcPlugin.Server.SessionManager.SummonPlayer(ghostID, NPRoomID);
            string responseData = await PrepareResponse(httpContext, 0x15, sessionData);
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/summonBlackGhost.spd")]
        public static async Task SummonBlackGhost(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("NPRoomID", out string NPRoomID)) throw new HttpException(500, "Missing NPRoomID.");

            byte[] sessionData = HtcPlugin.Server.SessionManager.SummonBlackGhost(NPRoomID);
            string responseData = await PrepareResponse(httpContext, 0x15, sessionData);
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/initializeMultiPlay.spd")]
        public static async Task InitializeMultiPlay(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            await player.InitializeMultiPlay();
            string responseData = await PrepareResponse(httpContext, 0x15, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/finalizeMultiPlay.spd")]
        public static async Task FinalizeMultiPlay(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            uint gradeS = 0, gradeA = 0, gradeB = 0, gradeC = 0, gradeD = 0;

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (data.TryGetValue("gradeS", out string gradeSRaw)) {
                uint.TryParse(gradeSRaw, out gradeS);
                if (gradeS > 0) gradeS = 1;
            }
            if (data.TryGetValue("gradeA", out string gradeARaw)) {
                uint.TryParse(gradeARaw, out gradeA);
                if (gradeA > 0) gradeA = 1;
            }
            if (data.TryGetValue("gradeB", out string gradeBRaw)) {
                uint.TryParse(gradeBRaw, out gradeB);
                if (gradeB > 0) gradeB = 1;
            }
            if (data.TryGetValue("gradeC", out string gradeCRaw)) {
                uint.TryParse(gradeCRaw, out gradeC);
                if (gradeC > 0) gradeC = 1;
            }
            if (data.TryGetValue("gradeD", out string gradeDRaw)) {
                uint.TryParse(gradeDRaw, out gradeD);
                if (gradeD > 0) gradeD = 1;
            }
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID(playerId, out var player)) throw new HttpException(500, "Failed to get player, probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player);
            await player.FinalizeMultiPlay(gradeS, gradeA, gradeB, gradeC, gradeD);
            string responseData = await PrepareResponse(httpContext, 0x21, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }

        [HttpPost("/cgi-bin/updateOtherPlayerGrade.spd")]
        public static async Task UpdateOtherPlayerGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);

            uint gradeS = 0, gradeA = 0, gradeB = 0, gradeC = 0, gradeD = 0;

            if (!data.TryGetValue("characterID", out string playerId)) throw new HttpException(500, "Missing characterID.");
            if (data.TryGetValue("gradeS", out string gradeSRaw)) {
                uint.TryParse(gradeSRaw, out gradeS);
                if (gradeS > 0) gradeS = 1;
            }
            if (data.TryGetValue("gradeA", out string gradeARaw)) {
                uint.TryParse(gradeARaw, out gradeA);
                if (gradeA > 0) gradeA = 1;
            }
            if (data.TryGetValue("gradeB", out string gradeBRaw)) {
                uint.TryParse(gradeBRaw, out gradeB);
                if (gradeB > 0) gradeB = 1;
            }
            if (data.TryGetValue("gradeC", out string gradeCRaw)) {
                uint.TryParse(gradeCRaw, out gradeC);
                if (gradeC > 0) gradeC = 1;
            }
            if (data.TryGetValue("gradeD", out string gradeDRaw)) {
                uint.TryParse(gradeDRaw, out gradeD);
                if (gradeD > 0) gradeD = 1;
            }
            if (!HtcPlugin.Server.PlayerManager.GetPlayerByNPID($"{playerId}0", out var player)) throw new HttpException(500, "Failed to get player, is he probably offline?");

            HtcPlugin.Server.PlayerManager.Heartbeat(player, true);
            await player.UpdateMultiPlay(gradeS, gradeA, gradeB, gradeC, gradeD);
            string responseData = await PrepareResponse(httpContext, 0x2b, new[] { (byte)'\x01' });
            await SendResponse(httpContext, responseData);
        }
    }
}
