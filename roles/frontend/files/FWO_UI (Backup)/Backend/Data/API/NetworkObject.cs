﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FWO.Ui.Data.Api
{
    public class NetworkObject
    {
        [JsonPropertyName("obj_ip")]
        public IPAddress IP { get; set; }
    }
}
