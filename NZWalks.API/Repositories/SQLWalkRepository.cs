using Microsoft.EntityFrameworkCore;
using NZWalks.API.Data;
using NZWalks.API.Models.Domain;
using NZWalks.API.Models.DTO;

namespace NZWalks.API.Repositories
{
    public class SQLWalkRepository : IWalkRepository
    {
        private readonly NZWalksDbContext dbContext;
        public SQLWalkRepository(NZWalksDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Walk> CreateAsync(Walk request)
        {
            var walk = new Walk
            {
                Name = request.Name,
                Description = request.Description,
                LengthInKm = request.LengthInKm,
                WalkImageUrl = request.WalkImageUrl,
                DifficultyId = request.DifficultyId,
                RegionId = request.RegionId,
            };
            await dbContext.Walks.AddAsync(walk);
            await dbContext.SaveChangesAsync();
            return walk;
        }
        public async Task<List<Walk>> GetAllAsync(string? filterOn = null, string? filterQuery = null,
            string? sortBy = null, bool isAscending = true,
            int page = 1, int pageSize = 100)
        {
            // return await dbContext.Walks.Include(x => x.Difficulty).Include(x => x.Region).ToListAsync();
            var walks = dbContext.Walks.Include(x => x.Difficulty).Include(x => x.Region).AsQueryable();
            // Filtering
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    walks = walks.Where(w => w.Name.Contains(filterQuery));
                }
            }
            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Name", StringComparison.OrdinalIgnoreCase))
                {
                    walks = isAscending ? walks.OrderBy(w => w.Name) : walks.OrderByDescending(w => w.Name);
                }
                else if (sortBy.Equals("Length", StringComparison.OrdinalIgnoreCase))
                {
                    walks = isAscending ? walks.OrderBy(w => w.LengthInKm) : walks.OrderByDescending(w => w.LengthInKm);
                }
            }
            // Pagination
            int skip = (page - 1) * pageSize;

            return await walks.Skip(skip).Take(pageSize).ToListAsync();
        }

        public async Task<Walk?> GetByIdAsync(Guid id)
            => await dbContext.Walks.Include(x => x.Difficulty)
                                     .Include(x => x.Region).FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Walk?> UpdateAsync(Guid id, Walk request)
        {
            var walk = await dbContext.Walks.FirstOrDefaultAsync(x => x.Id == id);
            if (walk is null)
            {
                return null;
            }
            walk.Name = request.Name;
            walk.Description = request.Description;
            walk.LengthInKm = request.LengthInKm;
            walk.DifficultyId = request.DifficultyId;
            walk.RegionId = request.RegionId;
            await dbContext.SaveChangesAsync();
            return walk;
        }
        public async Task<Walk?> DeleteAsync(Guid id)
        {
            var walk = await dbContext.Walks.FirstOrDefaultAsync(x => x.Id == id);
            if (walk is not null)
            {
                dbContext.Walks.Remove(walk);
                await dbContext.SaveChangesAsync();
                return walk;
            }
            return null;
        }
    }
}