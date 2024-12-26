using WebSitem.Models;

namespace WebSitem.Extensions
{
    public static class StorageExtensions
    {
        public static bool HasAvailableStorage(this AppUser user, long fileSize)
        {
            return (user.UsedStorage + fileSize) <= user.StorageLimit;
        }

        public static string GetFormattedStorageInfo(this AppUser user)
        {
            double usedGB = (double)user.UsedStorage / (1024 * 1024 * 1024);
            double totalGB = (double)user.StorageLimit / (1024 * 1024 * 1024);
            return $"{usedGB:F2}GB / {totalGB:F2}GB";
        }

        public static double GetStoragePercentage(this AppUser user)
        {
            return ((double)user.UsedStorage / user.StorageLimit) * 100;
        }
    }
}
