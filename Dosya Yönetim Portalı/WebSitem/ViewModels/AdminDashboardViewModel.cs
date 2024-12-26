public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalFolders { get; set; }
    public int TotalFiles { get; set; }
    public string TotalStorageUsed { get; set; }
    public List<UserManagementViewModel> Users { get; set; }
} 