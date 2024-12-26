using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using WebSitem.Repositories;
using WebSitem.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebSitem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace WebSitem.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private readonly INotyfService _notyfService;
        private readonly IMapper _mapper;
        private readonly UserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly FolderRepository _folderRepository;
        private readonly FileRepository _fileRepository;

        public AdminController(
            INotyfService notyfService, 
            IMapper mapper, 
            UserRepository userRepository,
            UserManager<AppUser> userManager,
            FolderRepository folderRepository,
            FileRepository fileRepository)
        {
            _notyfService = notyfService;
            _mapper = mapper;
            _userRepository = userRepository;
            _userManager = userManager;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");
            var userViewModels = new List<UserManagementViewModel>();
            long totalStorage = 0;
            int totalFolders = 0;
            int totalFiles = 0;

            foreach (var user in users)
            {
                var folders = await _folderRepository.Where(f => f.UserId == user.Id).ToListAsync();
                var files = await _fileRepository.Where(f => f.UserId == user.Id).ToListAsync();

                userViewModels.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    UsedStorage = user.UsedStorage,
                    StorageLimit = user.StorageLimit,
                    
                    FolderCount = folders.Count,
                    FileCount = files.Count
                });

                totalStorage += user.UsedStorage;
                totalFolders += folders.Count;
                totalFiles += files.Count;
            }

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                TotalFolders = totalFolders,
                TotalFiles = totalFiles,
                TotalStorageUsed = FormatSize(totalStorage),
                Users = userViewModels
            };

            return View(viewModel);
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        [HttpGet]
        public async Task<IActionResult> UsersManager()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");
            var viewModel = users.Select(u => new UserManagementViewModel
            {
                Id = u.Id,
                Email = u.Email,
                UsedStorage = u.UsedStorage,
                StorageLimit = u.StorageLimit,
               
            }).ToList();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStorageLimit(string userId, long newLimit)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı!" });
                }

                user.StorageLimit = newLimit * 1024 * 1024 * 1024; // GB'ı byte'a çevir
                await _userManager.UpdateAsync(user);

                return Json(new { 
                    success = true, 
                    message = "Depolama limiti güncellendi!", 
                    newLimit = newLimit,
                    formattedLimit = $"{newLimit} GB"
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Bir hata oluştu!" });
            }
        }
    }
}
