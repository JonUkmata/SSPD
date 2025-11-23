using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecommendationService.Data;
using RecommendationService.Models;

namespace RecommendationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecommendationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/recommendations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recommendation>>> GetRecommendations()
        {
            return await _context.Recommendations.ToListAsync();
        }

        // GET: api/recommendations/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Recommendation>> GetRecommendation(Guid id)
        {
            var recommendation = await _context.Recommendations.FindAsync(id);

            if (recommendation == null)
            {
                return NotFound();
            }

            return recommendation;
        }

        // GET: api/recommendations/business/{businessId}
        [HttpGet("business/{businessId}")]
        public async Task<ActionResult<IEnumerable<Recommendation>>> GetRecommendationsByBusiness(Guid businessId)
        {
            return await _context.Recommendations
                .Where(r => r.BusinessId == businessId)
                .OrderByDescending(r => r.Priority)
                .ToListAsync();
        }

        // POST: api/recommendations
        [HttpPost]
        public async Task<ActionResult<Recommendation>> PostRecommendation(Recommendation recommendation)
        {
            recommendation.Id = Guid.NewGuid();
            recommendation.CreatedAt = DateTime.UtcNow;

            _context.Recommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecommendation), new { id = recommendation.Id }, recommendation);
        }

        // PUT: api/recommendations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecommendation(Guid id, Recommendation recommendation)
        {
            if (id != recommendation.Id)
            {
                return BadRequest();
            }

            _context.Entry(recommendation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecommendationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/recommendations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecommendation(Guid id)
        {
            var recommendation = await _context.Recommendations.FindAsync(id);
            if (recommendation == null)
            {
                return NotFound();
            }

            _context.Recommendations.Remove(recommendation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RecommendationExists(Guid id)
        {
            return _context.Recommendations.Any(e => e.Id == id);
        }
    }
}
