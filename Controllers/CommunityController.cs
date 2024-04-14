using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reddit.Dtos;
using Reddit.Mapper;
using Reddit.Models;


namespace Reddit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommunityController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Community>>> GetCommunities(int pageNumber, int pageSize, string sortTerm = "id", bool isAssending = true, string searchKey = null)
        {
            var orderBy = string.IsNullOrWhiteSpace(sortTerm) ? "id" : sortTerm.ToLower();

            var query = _context.Communities.AsQueryable();

     
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                query = query.Where(c => c.Name.Contains(searchKey) || c.Description.Contains(searchKey));
            }
    
            var communities = query.Select(c => new
            {
                Community = c,
                PostsCount = c.Posts.Count(),
                SubscribersCount = c.Subscribers.Count()
            });
         
            switch (orderBy)
            {
                case "createdat":
                    communities = isAssending ?
                        communities.OrderBy(c => c.Community.CreateAt) :
                        communities.OrderByDescending(c => c.Community.CreateAt);
                    break;
                case "postscount":
                    communities = isAssending ?
                        communities.OrderBy(c => c.PostsCount) :
                        communities.OrderByDescending(c => c.PostsCount);
                    break;
                case "subscriberscount":
                    communities = isAssending ?
                        communities.OrderBy(c => c.SubscribersCount) :
                        communities.OrderByDescending(c => c.SubscribersCount);
                    break;
                case "id":
                default:
                    communities = isAssending ?
                        communities.OrderBy(c => c.Community.Id) :
                        communities.OrderByDescending(c => c.Community.Id);
                    break;
            }
         
            var pagedCommunities = await communities.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(c => c.Community).ToListAsync();
            return pagedCommunities;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Community>> GetCommunity(int id)
        {
            var community = await _context.Communities.FindAsync(id);

            if(community == null)
            {
                return NotFound();
            }

            return community;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCommunity(CreateCommunityDto communityDto)
        {
                var community = _mapper.toCommunity(communityDto);

                await _context.Communities.AddAsync(community);
                await _context.SaveChangesAsync();
                return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCommunity(int id)
        {
            var community = await _context.Communities.FindAsync(id);
            if (community == null)
            {
                return NotFound();
            }

            _context.Communities.Remove(community);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommunity (int id, Community community)
        {
            if (!CommunityExists(id))
            {
                return NotFound();
            }

            _context.Entry(community).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool CommunityExists(int id) => _context.Communities.Any(e => e.Id == id);
    }
}
