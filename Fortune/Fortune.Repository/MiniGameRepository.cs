using Fortune.Repository.Basic;
using Fortune.Repository.DBContext;
using Fortune.Repository.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fortune.Repository
{
    public class MiniGameRepository : GenericRepository<Mini_game>
    {
        public MiniGameRepository(FortuneContext context) : base(context)
        {
        }
        public async Task<Mini_game?> GetMiniGameByIdAsync(Guid id)
        {
            return await _context.Mini_games
                .AsNoTracking()
                .FirstOrDefaultAsync(mg => mg.miniGame_id == id);
        }

        public async Task<List<Mini_game>> GetAllMiniGamesAsync()
        {
            return await _context.Mini_games.ToListAsync();
        }
    }
}
