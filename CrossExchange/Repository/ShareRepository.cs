using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CrossExchange
{
    public class ShareRepository : GenericRepository<HourlyShareRate>, IShareRepository
    {
        public ShareRepository(ExchangeContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}