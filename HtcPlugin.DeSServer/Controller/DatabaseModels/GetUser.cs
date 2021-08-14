using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HtcSharp.HttpModule.Abstractions.Mvc;
using HtcSharp.HttpModule.Http;

namespace HtcPlugin.DeSServer.Controller.DatabaseModels {
    public class GetUser : IHttpJsonObject {

        [JsonPropertyName("username")]
        public string Username { get; set; }

        public Task<bool> ValidateData(HtcHttpContext httpContext) {
            if (string.IsNullOrEmpty(Username)) throw new ValidationException("Missing username.");
            return Task.FromResult(true);
        }
    }
}