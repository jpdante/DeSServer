using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HtcSharp.HttpModule.Http.Abstractions;
using HtcSharp.HttpModule.Mvc;

namespace HtcPlugin.DeSServer.Controller.DatabaseModels {
    public class GetReplays : IHttpJsonObject {

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("from")]
        public int From { get; set; }

        [JsonPropertyName("to")]
        public int To { get; set; }

        public Task<bool> ValidateData(HttpContext httpContext) {
            if (string.IsNullOrEmpty(Username)) throw new ValidationException("Missing username.");
            if (From < 0) throw new ValidationException("field 'from' needs to be at least 0.");
            if (To <= 0) throw new ValidationException("field 'to' cannot be higher than 100.");
            return Task.FromResult(true);
        }
    }
}