namespace WebSitem.Models
{
    public class File
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; } // byte cinsinden dosya boyutu
        public DateTime UploadedAt { get; set; }
        public bool IsDeleted { get; set; } // Soft delete için
        
        // İlişkiler
        public string UserId { get; set; }
        public AppUser User { get; set; }
        
        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        
        public int? CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
