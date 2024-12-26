using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebSitem.Models;

namespace WebSitem.Repositories
{
    public class FolderRepository
    {
        private readonly AppDbContext _context;

        public FolderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Folder>> GetAllAsync()
        {
            return await _context.Folders.ToListAsync();
        }

        public async Task<Folder> GetByIdAsync(int id)
        {
            return await _context.Folders.FindAsync(id);
        }

        public async Task AddAsync(Folder folder)
        {
            await _context.Folders.AddAsync(folder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Folder folder)
        {
            _context.Folders.Update(folder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var folder = await GetByIdAsync(id);
            if (folder != null)
            {
                _context.Folders.Remove(folder);
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<Folder> Where(Expression<Func<Folder, bool>> expression)
        {
            return _context.Folders.Where(expression);
        }
    }
}
