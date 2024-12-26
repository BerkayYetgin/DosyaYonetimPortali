namespace WebSitem.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public ICollection<File> Files { get; set; }
    }
}
