﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Models
{
    public class User
    {
        [JsonProperty("user")]
        public UserInfo UserDetail { get; set; }
    }
}
