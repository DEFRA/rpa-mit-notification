using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using RPA.MIT.Notification.Function.Models;

namespace RPA.MIT.Notification.Function.Services;

public class NotificationTable : INotificationTable
{
    private readonly TableClient _tableClient;

    public NotificationTable(TableClient tableClient)
    {
        tableClient.CreateIfNotExists();
        _tableClient = tableClient;
        _tableClient.CreateIfNotExists();
    }

    public async Task Add(NotificationEntity entity)
    {
        await _tableClient.AddEntityAsync(entity);
    }

    public Azure.Pageable<NotificationEntity> RetrieveActive()
    {
        return _tableClient.Query<NotificationEntity>(filter: "Status eq 'sent'");
    }

    public async Task Delete(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task Update(TableEntity entity)
    {
        await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
    }

    public async Task<TableEntity> Get(string partitionKey, string rowKey)
    {
        return await _tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
    }
}