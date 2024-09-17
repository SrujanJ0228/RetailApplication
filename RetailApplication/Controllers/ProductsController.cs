using Microsoft.AspNetCore.Mvc;
using RetailApplication.Data;
using RetailApplication.Models;

namespace RetailApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public IActionResult GetProducts([FromQuery] string name, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.Products.Where(p => p.IsActive).OrderByDescending(p => p.CreatedDate).AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.ProductName.Contains(name));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate <= endDate.Value);
            }

            return Ok(query.ToList());
        }

        // POST: api/products
        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            if (product.Price > 10000)
            {
                return BadRequest("Product creation is not allowed for prices above $10,000.");
            }

            if (product.Price > 5000)
            {
                product.Status = "PendingApproval";
                _context.ApprovalQueue.Add(new ApprovalQueue
                {
                    ProductId = product.ProductId,
                    RequestReason = "Price exceeds $5000",
                    ActionType = "Create"
                });
            }
            else
            {
                product.Status = "Active";
            }

            product.CreatedDate = DateTime.Now;
            product.IsActive = true;

            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            if (updatedProduct.Price > 10000)
            {
                return BadRequest("Product update is not allowed for prices above $10,000.");
            }

            if (updatedProduct.Price > 5000 || updatedProduct.Price > product.Price * 1.5M)
            {
                updatedProduct.Status = "PendingApproval";
                _context.ApprovalQueue.Add(new ApprovalQueue
                {
                    ProductId = product.ProductId,
                    RequestReason = updatedProduct.Price > 5000 ? "Price exceeds $5000" : "Price increased more than 50%",
                    ActionType = "Update"
                });
            }
            else
            {
                updatedProduct.Status = "Active";
            }

            product.ProductName = updatedProduct.ProductName;
            product.Price = updatedProduct.Price;
            product.LastUpdatedDate = DateTime.Now;
            _context.SaveChanges();

            return Ok(product);
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            product.IsActive = false;
            product.Status = "PendingApproval";

            _context.ApprovalQueue.Add(new ApprovalQueue
            {
                ProductId = product.ProductId,
                RequestReason = "Product Deletion",
                ActionType = "Delete"
            });

            _context.SaveChanges();

            return NoContent();
        }
    }

}
