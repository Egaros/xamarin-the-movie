﻿using Newtonsoft.Json;

namespace TheMovie.Models
{
    public class Spoken_Languages
    {
        [JsonProperty(PropertyName = "iso_639_1")]
        public string Iso6391 { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

}