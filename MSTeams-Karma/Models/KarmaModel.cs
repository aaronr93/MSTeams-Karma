using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MSTeams.Karma.Models
{
    public class KarmaModel
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }
    }
}