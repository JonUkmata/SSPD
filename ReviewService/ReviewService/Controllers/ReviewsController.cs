using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Models;
using ReviewService.Messaging;
using ReviewService.Events;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Func<RabbitMQPublisher>? _publisherFactory;

        public ReviewsController(ApplicationDbContext context, Func<RabbitMQPublisher>? publisherFactory = null)
        {
            _context = context;
            _publisherFactory = publisherFactory;
        }

        // GET: api/reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        // GET: api/reviews/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            try
            {
                Console.WriteLine($"[ReviewsController] Creating review for business {review.BusinessId}");
                review.Id = Guid.NewGuid();
                review.CreatedAt = DateTime.UtcNow;

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[ReviewsController] Review saved to database: {review.Id}");

                // Publish event to RabbitMQ
                var reviewCreatedEvent = new ReviewCreatedEvent
                {
                    Id = review.Id,
                    BusinessId = review.BusinessId,
                    UserId = review.UserId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                Console.WriteLine($"[ReviewsController] Creating publisher...");
                if (_publisherFactory != null)
                {
                    using (var publisher = _publisherFactory())
                    {
                        Console.WriteLine($"[ReviewsController] Publishing event...");
                        await publisher.PublishAsync(reviewCreatedEvent);
                        Console.WriteLine($"[ReviewsController] Event published successfully");
                    }
                }
                else
                {
                    Console.WriteLine($"[ReviewsController] WARNING: No publisher factory configured, skipping event publishing");
                }

                return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReviewsController] ERROR: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[ReviewsController] Inner: {ex.InnerException.Message}");
                Console.WriteLine($"[ReviewsController] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // PUT: api/reviews/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(Guid id, Review review)
        {
            if (id != review.Id)
            {
                return BadRequest();
            }

            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
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

        // DELETE: api/reviews/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(Guid id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
