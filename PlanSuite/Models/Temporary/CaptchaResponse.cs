using Newtonsoft.Json;

namespace PlanSuite.Models.Temporary
{
    public class CaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }
    }
}
