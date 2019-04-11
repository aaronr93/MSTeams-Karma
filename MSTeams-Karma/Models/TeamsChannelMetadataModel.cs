using Newtonsoft.Json;

namespace MSTeams.Karma.Models
{
    public class TeamsChannelMetadataModel
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("teamid")]
        public string TeamId { get; set; }
    }
}