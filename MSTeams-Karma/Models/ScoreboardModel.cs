using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MSTeams.Karma.Models
{
    public class ScoreboardModel
    {
        [JsonProperty("feed", Required = Required.Always)]
        public List<KarmaModel> Feed { get; set; }
    }
}