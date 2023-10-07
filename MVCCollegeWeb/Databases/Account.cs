using System.Text.Json.Serialization;

namespace MVCCollegeWeb.Databases
{
    public class Account
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
