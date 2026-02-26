using isc.time.report.be.application.Interfaces.Repository.Leaders;
using isc.time.report.be.domain.Entity.Leaders;
using isc.time.report.be.domain.Entity.Shared;
using isc.time.report.be.domain.Exceptions;
using isc.time.report.be.infrastructure.Database;
using isc.time.report.be.infrastructure.Utils.Pagination;
using Microsoft.EntityFrameworkCore;

namespace isc.time.report.be.infrastructure.Repositories.Leaders
{
    public class LeaderRepository : ILeaderRepository
    {
        private readonly DBContext _dbContext;

        public LeaderRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Leader>> GetAllLeadersPaginatedAsync(PaginationParams paginationParams, string? search)
        {
            var query = _dbContext.Leaders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string normalizedSearch = search.Trim();

                query = query.Where(l =>
                    (l.FirstName != null && l.FirstName.Contains(normalizedSearch)) ||
                    (l.LastName != null && l.LastName.Contains(normalizedSearch))
                );
            }

            return await PaginationHelper.CreatePagedResultAsync(query, paginationParams);
        }

        public async Task<Leader> GetLeaderByIDAsync(int leaderId)
        {
            if (leaderId <= 0)
            {
                throw new ClientFaultException("El ID de la Leader no puede ser menor o igual a 0");
            }
            var leader = await _dbContext.Leaders
                .FirstOrDefaultAsync(e => e.Id == leaderId);
            if (leader == null)
            {
                throw new ClientFaultException($"No se encontró un líder con ID {leaderId}.");
            }
            return leader;
        }

        public async Task<Leader> CreateLeaderAsync(Leader leader)
        {
            leader.CreationDate = DateTime.Now;
            leader.ModificationDate = null;
            leader.Status = true;
            leader.CreationUser = "SYSTEM"; // Or from context

            await _dbContext.Leaders.AddAsync(leader);
            await _dbContext.SaveChangesAsync();

            return leader;
        }

        public async Task<Leader> UpdateLeaderAsync(Leader leader)
        {
            leader.ModificationDate = DateTime.Now;
            _dbContext.Entry(leader).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return leader;
        }

        public async Task<Leader> InactivateLeaderAsync(int leaderId)
        {
            var leader = await _dbContext.Leaders.FirstOrDefaultAsync(e => e.Id == leaderId);
            if (leader == null)
                throw new ClientFaultException($"El líder con ID {leaderId} no existe.");

            leader.Status = false;
            leader.ModificationDate = DateTime.Now;
            _dbContext.Entry(leader).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return leader;
        }

        public async Task<Leader> ActivateLeaderAsync(int leaderId)
        {
            var leader = await _dbContext.Leaders.FirstOrDefaultAsync(e => e.Id == leaderId);
            if (leader == null)
                throw new ClientFaultException($"El líder con ID {leaderId} no existe.");

            leader.Status = true;
            leader.ModificationDate = DateTime.Now;
            _dbContext.Entry(leader).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return leader;
        }

        public async Task<List<Leader>> GetActiveLeadersByProjectIdsAsync(List<int> projectIds)
        {
            if (projectIds == null || !projectIds.Any())
            {

                return new List<Leader>();
            }

            var leaders = await _dbContext.Projects
                .Where(p => projectIds.Contains(p.Id) && p.Leader != null)
                .Select(p => p.Leader)
                .Distinct()
                .ToListAsync();

            return leaders;
        }

        public async Task SaveLeadersAsync(List<Leader> leaders)
        {
            foreach (var leader in leaders)
            {
                if (leader.Id == 0)
                    await _dbContext.Leaders.AddAsync(leader);
                else
                    _dbContext.Leaders.Update(leader);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Dictionary<int, Leader>> GetLeadersByProjectIdsDictionaryAsync(List<int> projectIds)
        {
            if (projectIds == null || !projectIds.Any())
            {
                return new Dictionary<int, Leader>();
            }

            var projectLeaders = await _dbContext.Projects
                .Where(p => projectIds.Contains(p.Id) && p.Leader != null)
                .Select(p => new { p.Id, p.Leader })
                .ToListAsync();

            return projectLeaders.ToDictionary(k => k.Id, v => v.Leader);
        }
    }
}
