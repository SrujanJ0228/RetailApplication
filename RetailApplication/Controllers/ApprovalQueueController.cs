using Microsoft.AspNetCore.Mvc;
using RetailApplication.Data;

namespace RetailApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalQueueController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApprovalQueueController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/approvalqueue
        [HttpGet]
        public IActionResult GetApprovalQueue()
        {
            var queue = _context.ApprovalQueue
                .Join(_context.Products, aq => aq.ProductId, p => p.ProductId,
                    (aq, p) => new
                    {
                        p.ProductName,
                        aq.RequestReason,
                        aq.RequestDate,
                        aq.ActionType
                    })
                .OrderBy(aq => aq.RequestDate)
                .ToList();

            return Ok(queue);
        }

        // PUT: api/approvalqueue/approve/{id}
        [HttpPut("approve/{id}")]
        public IActionResult Approve(int id)
        {
            var approval = _context.ApprovalQueue.FirstOrDefault(aq => aq.ApprovalQueueId == id);
            if (approval == null) return NotFound();

            var product = _context.Products.FirstOrDefault(p => p.ProductId == approval.ProductId);
            if (product == null) return NotFound();

            product.Status = approval.ActionType == "Delete" ? "Deleted" : "Active";
            _context.ApprovalQueue.Remove(approval);

            _context.SaveChanges();

            return Ok();
        }

        // PUT: api/approvalqueue/reject/{id}
        [HttpPut("reject/{id}")]
        public IActionResult Reject(int id)
        {
            var approval = _context.ApprovalQueue.FirstOrDefault(aq => aq.ApprovalQueueId == id);
            if (approval == null) return NotFound();

            _context.ApprovalQueue.Remove(approval);
            _context.SaveChanges();

            return Ok();
        }
    }

}
