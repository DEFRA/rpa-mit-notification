using System;
using Azure;
using Azure.Data.Tables;

namespace RPA.MIT.Notification.Function.Models
{
    public class NotificationEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Status { get; set; }
        public string NotifyId { get; set; }
        public int RetryCount { get; set; }
        public string Data { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
