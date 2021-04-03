using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HtcSharp.Core.Logging.Abstractions;
using HtcSharp.HttpModule.Http.Abstractions;
using HtcSharp.HttpModule.Http.Abstractions.Extensions;
using HtcSharp.HttpModule.Mvc;
using Microsoft.Extensions.Primitives;

namespace HtcPlugin.DeSServer.Controller {
    public class DeSController {

        private static async Task<string> PrepareResponse(HttpContext httpContext, byte cmd, byte[] data) {
            var memoryStream = new MemoryStream();
            memoryStream.WriteByte(cmd);
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) data.Length + 5));
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
            HtcPlugin.Logger.LogInfo($"{httpContext.Request.Method} {httpContext.Request.Path} {{");
            HtcPlugin.Logger.LogInfo($"    Host: {httpContext.Request.Host}");
            HtcPlugin.Logger.LogInfo($"    ContentType: {httpContext.Request.ContentType}");
            HtcPlugin.Logger.LogInfo($"    Query: {httpContext.Request.QueryString}");
            HtcPlugin.Logger.LogInfo($"    Header: {{");
            foreach ((string key, var value) in httpContext.Request.Headers) {
                HtcPlugin.Logger.LogInfo($"        {key}: \"{value}\"");
            }
            HtcPlugin.Logger.LogInfo($"    }},");
            if (data != null)  HtcPlugin.Logger.LogInfo($"    Body: {data}");
            HtcPlugin.Logger.LogInfo($"}}");
        }

        [HttpPost("/demons-souls-us/ss.info")]
        public static async Task SsInfo(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
            httpContext.Response.StatusCode = 200;
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
                "<gameurl1>http://127.0.0.1:18000/cgi-bin/</gameurl1>\r\n" +
                "<gameurl2>http://127.0.0.1:18000/cgi-bin/</gameurl2>\r\n" +
                "<gameurl3>http://127.0.0.1:18000/cgi-bin/</gameurl3>\r\n" +
                "<gameurl4>http://127.0.0.1:18000/cgi-bin/</gameurl4>\r\n" +
                "<gameurl5>http://127.0.0.1:18000/cgi-bin/</gameurl5>\r\n" +
                "<gameurl6>http://127.0.0.1:18000/cgi-bin/</gameurl6>\r\n" +
                "<gameurl7>http://127.0.0.1:18000/cgi-bin/</gameurl7>\r\n" +
                "<gameurl8>http://127.0.0.1:18000/cgi-bin/</gameurl8>\r\n" +
                "<gameurl11>http://127.0.0.1:18000/cgi-bin/</gameurl11>\r\n" +
                "<gameurl12>http://127.0.0.1:18000/cgi-bin/</gameurl12>\r\n" +
                "<browserurl1></browserurl1>\r\n" +
                "<browserurl2></browserurl2>\r\n" +
                "<browserurl3></browserurl3>\r\n" +
                "<interval1>120</interval1>\r\n" +
                "<interval2>120</interval2>\r\n" +
                "<interval3>120</interval3>\r\n" +
                "<interval4>120</interval4>\r\n" +
                "<interval5>120</interval5>\r\n" +
                "<interval6>120</interval6>\r\n" +
                "<interval7>120</interval7>\r\n" +
                "<interval8>120</interval8>\r\n" +
                "<interval11>120</interval11>\r\n" +
                "<interval12>120</interval12>\r\n" +
                "<getWanderingGhostInterval>20</getWanderingGhostInterval>\r\n" +
                "<setWanderingGhostInterval>20</setWanderingGhostInterval>\r\n" +
                "<getBloodMessageNum>80</getBloodMessageNum>\r\n" +
                "<getReplayListNum>80</getReplayListNum>\r\n" +
                "<enableWanderingGhost>1</enableWanderingGhost>");
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
        }

        [HttpPost("/cgi-bin/getQWCData.spd")]
        public static async Task GetQWCData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/addQWCData.spd")]
        public static async Task AddQWCData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getMultiPlayGrade.spd")]
        public static async Task GetMultiPlayGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getBloodMessageGrade.spd")]
        public static async Task GetBloodMessageGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getTimeMessage.spd")]
        public static async Task GetTimeMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getAgreement.spd")]
        public static async Task GetAgreement(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/addNewAccount.spd")]
        public static async Task AddNewAccount(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getBloodMessage.spd")]
        public static async Task GetBloodMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/addBloodMessage.spd")]
        public static async Task AddBloodMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/updateBloodMessageGrade.spd")]
        public static async Task UpdateBloodMessageGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/deleteBloodMessage.spd")]
        public static async Task DeleteBloodMessage(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getReplayList.spd")]
        public static async Task GetReplayList(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getReplayData.spd")]
        public static async Task GetReplayData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/addReplayData.spd")]
        public static async Task AddReplayData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getWanderingGhost.spd")]
        public static async Task GetWanderingGhost(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/setWanderingGhost.spd")]
        public static async Task SetWanderingGhost(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/getSosData.spd")]
        public static async Task GetSosData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/addSosData.spd")]
        public static async Task AddSosData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/checkSosData.spd")]
        public static async Task CheckSosData(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/outOfBlock.spd")]
        public static async Task OutOfBlock(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/summonOtherCharacter.spd")]
        public static async Task SummonOtherCharacter(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/summonBlackGhost.spd")]
        public static async Task SummonBlackGhost(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/initializeMultiPlay.spd")]
        public static async Task InitializeMultiPlay(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/finalizeMultiPlay.spd")]
        public static async Task FinalizeMultiPlay(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }

        [HttpPost("/cgi-bin/updateOtherPlayerGrade.spd")]
        public static async Task UpdateOtherPlayerGrade(HttpContext httpContext) {
            string dataRaw = await GetAndDecryptData(httpContext);
            PrintRequest(httpContext, dataRaw);
            Dictionary<string, string> data = ParamData(dataRaw);
        }
    }
}
