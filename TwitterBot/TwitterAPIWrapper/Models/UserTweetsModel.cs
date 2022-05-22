using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TwitterAPIWrapper.Models;


public class UserTweetsModel
{
    [JsonPropertyName("data")]
    public Datum[] Data { get; set; } = new Datum[0];
    [JsonPropertyName("includes")]
    public Includes Includes { get; set; } = new();
    [JsonPropertyName("meta")]
    public TweetsMeta Meta { get; set; } = new();
}

public class Includes
{
    [JsonPropertyName("users")]
    public User[] Users { get; set; } = new User[0];
}

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } 
}

public class TweetsMeta
{
    [JsonPropertyName("newest_id")]
    public string NewestID { get; set; } = string.Empty;
    [JsonPropertyName("oldest_id")]
    public string OldestID { get; set; } = string.Empty;
    [JsonPropertyName("result_count")]
    public int ResultCount { get; set; }
}

public class Datum
{
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;
    [JsonPropertyName("author_id")]
    public string AuthorID { get; set; } = string.Empty;
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

