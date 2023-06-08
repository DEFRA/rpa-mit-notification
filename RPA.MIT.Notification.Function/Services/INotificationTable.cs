using System.Threading.Tasks;
using Azure.Data.Tables;
using RPA.MIT.Notification.Function.Models;

namespace RPA.MIT.Notification.Function.Services;

public interface INotificationTable
{
    Azure.Pageable<NotificationEntity> RetrieveActive();

    Task Add(NotificationEntity entity);

    Task Delete(string partitionKey, string rowKey);

    Task Update(TableEntity entity);

    Task<TableEntity> Get(string partitionKey, string rowKey);
}