using System.Linq.Expressions;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositories;

/// <summary>پیاده‌سازی Generic Repository</summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly HotelDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(HotelDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.FirstOrDefaultAsync(predicate);

    public async Task AddAsync(T entity) =>
        await _dbSet.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) =>
        await _dbSet.AddRangeAsync(entities);

    public void Update(T entity)
    {
        entity.UpdatedAt = DateTime.Now;
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        // حذف منطقی به جای حذف فیزیکی
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.Now;
        _dbSet.Update(entity);
    }

    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
