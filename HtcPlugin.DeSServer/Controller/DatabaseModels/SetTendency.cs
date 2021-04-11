using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HtcSharp.HttpModule.Http.Abstractions;
using HtcSharp.HttpModule.Mvc;

namespace HtcPlugin.DeSServer.Controller.DatabaseModels {
    public class SetTendency : IHttpJsonObject {

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("desiredTendency")]
        public int DesiredTendency { get; set; }

        [JsonPropertyName("useDesired")]
        public bool UseDesired { get; set; }

        public Task<bool> ValidateData(HttpContext httpContext) {
            if (string.IsNullOrEmpty(Username)) throw new ValidationException("Missing username.");
            return Task.FromResult(true);
        }
    }
}