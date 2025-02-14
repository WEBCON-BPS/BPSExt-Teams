﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebCon.BpsExt.Teams.CustomActions.Models
{
    public class AuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    public class PrivilegesList
    {
        [JsonProperty("elementPrivileges")]
        public List<ElementPrivileges> Privileges;
    }

    public class ElementPrivileges
    {
        [JsonProperty("permissionsScope")]
        public string PermissionsScope { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

    }
    public class User
    {
        [JsonProperty("bpsId")]
        public string BpsId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
