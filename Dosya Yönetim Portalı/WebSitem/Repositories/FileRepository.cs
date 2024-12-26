using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebSitem.Models;
using t=WebSitem.Models;

namespace WebSitem.Repositories
{
    public class FileRepository
    {
        private readonly AppDbContext _context;

        public FileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<t.File>> GetAllAsync()
        {
            return await _context.Files.ToListAsync();
        }

        public async Task<t.File> GetByIdAsync(int id)
        {
            return await _context.Files.FindAsync(id);
        }

        public async Task AddAsync(t.File file)
        {
            await _context.Files.AddAsync(file);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(t.File file)
        {
            _context.Files.Update(file);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var file = await GetByIdAsync(id);
            if (file != null)
            {
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<t.File> Where(Expression<Func<t.File, bool>> expression)
        {
            return _context.Files.Where(expression);
        }
    }
}
