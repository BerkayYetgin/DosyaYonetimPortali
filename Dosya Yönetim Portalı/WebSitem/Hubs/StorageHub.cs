using Microsoft.AspNetCore.SignalR;

namespace WebSitem.Hubs
{
    public class StorageHub : Hub
    {
        public async Task UpdateStorageUsage(string userId, long usedStorage, long storageLimit)
        {
            await Clients.User(userId).SendAsync("ReceiveStorageUpdate", usedStorage, storageLimit);
        }
    }
} 