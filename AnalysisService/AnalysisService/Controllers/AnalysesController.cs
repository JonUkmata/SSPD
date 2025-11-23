using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnalysisService.Data;
using AnalysisService.Models;

namespace AnalysisService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalysesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/analyses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Analysis>>> GetAnalyses()
        {
            return await _context.Analyses.ToListAsync();
        }

        // GET: api/analyses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Analysis>> GetAnalysis(Guid id)
        {
            var analysis = await _context.Analyses.FindAsync(id);

            if (analysis == null)
            {
                return NotFound();
            }

            return analysis;
        }

        // GET: api/analyses/business/{businessId}
        [HttpGet("business/{businessId}")]
        public async Task<ActionResult<IEnumerable<Analysis>>> GetAnalysesByBusiness(Guid businessId)
        {
            return await _context.Analyses
                .Where(a => a.BusinessId == businessId)
                .ToListAsync();
        }

        // POST: api/analyses
        [HttpPost]
        public async Task<ActionResult<Analysis>> PostAnalysis(Analysis analysis)
        {
            analysis.Id = Guid.NewGuid();
            analysis.CreatedAt = DateTime.UtcNow;

            _context.Analyses.Add(analysis);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnalysis), new { id = analysis.Id }, analysis);
        }

        // PUT: api/analyses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnalysis(Guid id, Analysis analysis)
        {
            if (id != analysis.Id)
            {
                return BadRequest();
            }

            _context.Entry(analysis).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnalysisExists(id))
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

        // DELETE: api/analyses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnalysis(Guid id)
        {
            var analysis = await _context.Analyses.FindAsync(id);
            if (analysis == null)
            {
                return NotFound();
            }

            _context.Analyses.Remove(analysis);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AnalysisExists(Guid id)
        {
            return _context.Analyses.Any(e => e.Id == id);
        }
    }
}
