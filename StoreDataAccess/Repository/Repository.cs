using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StoreDataAccess.Repository.IRepository;

namespace StoreDataAccess.Repository
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly AppDbContext _db;
        internal DbSet<T> _dbSet;

        public Repository(AppDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<T> Get(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool tracked = false
        )
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = _dbSet;
            }
            else
            {
                query = _dbSet.AsNoTracking();
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (
                    var includeProperty in includeProperties.Split(
                        new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                {
                    query = query.Include(includeProperty.Trim());
                }
            }
            return await query.FirstOrDefaultAsync(filter);
        }

        public async Task<IEnumerable<T>> GetAll(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool tracked = false
        )
        {
            IQueryable<T> query = _dbSet;
            if (tracked)
            {
                query = _dbSet;
            }
            else
            {
                query = _dbSet.AsNoTracking();
            }
            if (filter is not null)
                query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (
                    var property in includeProperties.Split(
                        new char[] { ',' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                {
                    query = query.Include(property);
                }
            }
            return await query.ToListAsync();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
