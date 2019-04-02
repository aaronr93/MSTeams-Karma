using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MSTeams.Karma.Models
{
    public class KarmaModel
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("partition")]
        public string Partition { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }
    }
}