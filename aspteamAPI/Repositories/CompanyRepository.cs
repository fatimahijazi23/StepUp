
using aspteamAPI.context;
using aspteamAPI.IRepository;
using Microsoft.EntityFrameworkCore;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext  _context;

    public CompanyRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<CompanyAccount>> GetAllCompanies()
    {
        return await _context.CompanyAccounts
                             .AsNoTracking()
                             .ToListAsync();
    }


    public async Task<CompanyAccount?> GetCompanyById(int companyId)
    {
        return await _context.CompanyAccounts.SingleOrDefaultAsync( c=>c.Id == companyId);
    }

    public async Task<List<Follow>>? GetFollowers(int companyId)
    {
        return await _context.Follows.Where( f => f.CompanyId == companyId)
                                     .AsNoTracking()
                                     .ToListAsync();

    }
}