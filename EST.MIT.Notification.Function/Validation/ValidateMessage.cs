using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace EST.MIT.Notification.Function.Validation
{
    public static class ValidateMessage
    {
        const string schemaJson = @"{
        ""$schema"": ""http://json-schema.org/draft-07/schema#"",
        ""type"": ""object"",
        ""properties"": {
            ""Action"": {
                ""type"": ""string""
            },
            ""Data"": {
                ""type"": ""object""
            },
            ""Id"": {
                ""type"": ""string""
            },
            ""Scheme"": {
                ""type"": ""string""
            },
        },
        ""required"": [
            ""Action"",
            ""Data"",
            ""Id"",
            ""Scheme""
        ]
        }";

        public static bool IsValid(string notification)
        {
            var schema = JSchema.Parse(schemaJson);
            var parseNotification = JObject.Parse(notification);
            return parseNotification.IsValid(schema);
        }
    }
}
