public class UserManagementViewModel
{
    public string Id { get; set; }
    public string Email { get; set; }
    public long UsedStorage { get; set; }
    public long StorageLimit { get; set; }
    public string FormattedUsedStorage => FormatSize(UsedStorage);
    public string FormattedStorageLimit => FormatSize(StorageLimit);
    public int FolderCount { get; set; }
    public int FileCount { get; set; }


    private string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
} 