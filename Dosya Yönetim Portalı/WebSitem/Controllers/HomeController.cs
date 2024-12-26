using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebSitem.Models;
using WebSitem.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using WebSitem.Extensions;
using t= WebSitem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using WebSitem.Hubs;

namespace WebSitem.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly FolderRepository _folderRepository;
        private readonly FileRepository _fileRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotyfService _notyfService;
        private readonly IWebHostEnvironment _env;

        public HomeController(FolderRepository folderRepository, FileRepository fileRepository, UserManager<AppUser> userManager, INotyfService notyfService, IWebHostEnvironment env)
        {
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
            _userManager = userManager;
            _notyfService = notyfService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var folders = await _folderRepository.Where(f => f.UserId == user.Id).ToListAsync();
            
            ViewBag.UsedStorage = user.UsedStorage;
            ViewBag.StorageLimit = user.StorageLimit;
            
            return View(folders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder(string folderName)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var folder = new Folder
            {
                Name = folderName,
                CreatedAt = DateTime.Now,
                UserId = user.Id
            };

            await _folderRepository.AddAsync(folder);

            return Json(new { 
                success = true, 
                message = "Klasör başarıyla oluşturuldu",
                folder = new { 
                    id = folder.Id, 
                    name = folder.Name, 
                    createdAt = folder.CreatedAt.ToString("dd.MM.yyyy HH:mm") 
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Dosya seçilmedi" });

            if (!user.HasAvailableStorage(file.Length))
                return Json(new { success = false, message = "Yetersiz depolama alanı" });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", user.Id);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileEntity = new t.File
            {
                FileName = fileName,
                FilePath = filePath,
                FileSize = file.Length,
                UploadedAt = DateTime.Now,
                UserId = user.Id,
                FolderId = folderId
            };

            await _fileRepository.AddAsync(fileEntity);
            
            user.UsedStorage += file.Length;
            await _userManager.UpdateAsync(user);

            // SignalR ile storage kullanımını güncelle
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<StorageHub>>();
            await hubContext.Clients.User(user.Id).SendAsync("ReceiveStorageUpdate", user.UsedStorage, user.StorageLimit);

            return Json(new { 
                success = true, 
                message = "Dosya başarıyla yüklendi",
                file = new { 
                    id = fileEntity.Id,
                    name = fileEntity.FileName,
                    size = fileEntity.FileSize,
                    uploadedAt = fileEntity.UploadedAt.ToString("dd.MM.yyyy HH:mm")
                },
                usedStorage = user.UsedStorage,
                storageLimit = user.StorageLimit
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles(int folderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var files = await _fileRepository.Where(f => f.FolderId == folderId && f.UserId == user.Id)
                .Select(f => new {
                    id = f.Id,
                    fileName = f.FileName,
                    fileSize = f.FileSize,
                    uploadedAt = f.UploadedAt,
                    filePath = f.FilePath
                })
                .ToListAsync();

            return Json(new { 
                success = true, 
                files = files
            });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null)
            {
                _notyfService.Error("Dosya bulunamadı!");
                return Json(new { success = false });
            }

            var user = await _userManager.GetUserAsync(User);
            if (file.UserId != user.Id)
            {
                _notyfService.Error("Bu dosyaya erişim yetkiniz yok!");
                return Json(new { success = false });
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                _notyfService.Error("Dosya fiziksel olarak bulunamadı!");
                return Json(new { success = false });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", file.FileName);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null)
            {
                return Json(new { success = false, message = "Dosya bulunamadı!" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (file.UserId != user.Id)
            {
                return Json(new { success = false, message = "Bu dosyaya erişim yetkiniz yok!" });
            }

            // Fiziksel dosyayı sil
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }

            // Kullanıcının storage kullanımını güncelle
            user.UsedStorage -= file.FileSize;
            await _userManager.UpdateAsync(user);

            // Veritabanından dosyayı sil
            await _fileRepository.DeleteAsync(id);

            // SignalR ile storage kullanımını güncelle
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<StorageHub>>();
            await hubContext.Clients.User(user.Id).SendAsync("ReceiveStorageUpdate", user.UsedStorage, user.StorageLimit);

            return Json(new { 
                success = true, 
                message = "Dosya başarıyla silindi!",
                usedStorage = user.UsedStorage,
                storageLimit = user.StorageLimit
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);
            if (folder == null)
            {
                return Json(new { success = false, message = "Klasör bulunamadı!" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (folder.UserId != user.Id)
            {
                return Json(new { success = false, message = "Bu klasöre erişim yetkiniz yok!" });
            }

            // Klasördeki tüm dosyaları al
            var files = await _fileRepository.Where(f => f.FolderId == id).ToListAsync();
            long totalFreedSpace = 0;

            // Fiziksel dosyaları sil ve boşalan alanı hesapla
            foreach (var file in files)
            {
                if (System.IO.File.Exists(file.FilePath))
                {
                    System.IO.File.Delete(file.FilePath);
                }
                totalFreedSpace += file.FileSize;
            }

            // Kullanıcının storage kullanımını güncelle
            user.UsedStorage -= totalFreedSpace;
            await _userManager.UpdateAsync(user);

            // Klasörü ve içindeki dosyaları veritabanından sil
            await _folderRepository.DeleteAsync(id);

            // SignalR ile storage kullanımını güncelle
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<StorageHub>>();
            await hubContext.Clients.User(user.Id).SendAsync("ReceiveStorageUpdate", user.UsedStorage, user.StorageLimit);

            return Json(new { 
                success = true, 
                message = "Klasör ve içindeki tüm dosyalar başarıyla silindi!",
                usedStorage = user.UsedStorage,
                storageLimit = user.StorageLimit
            });
        }
    }
}
