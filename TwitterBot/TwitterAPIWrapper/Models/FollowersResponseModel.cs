using System.Text.Json.Serialization;

namespace TwitterAPIWrapper.Models
{
    public class FollowersResponseModel
    {
        [JsonPropertyName("data")]
        public FollowerDetails[] Data { get; set; } = new FollowerDetails[0];
        [JsonPropertyName("meta")]
        public Meta Meta { get; set; } = new();
    }

    public class Meta
    {
        [JsonPropertyName("result_count")]
        public int ResultCount { get; set; }
    }

    public class FollowerDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
    }
}
