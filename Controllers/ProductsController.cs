using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CartWebApi;
using CartWebApi.Data;
using CartWebApi.DTO;
using Microsoft.AspNetCore.Authorization;

namespace CartWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ProductsController(ApiDbContext context)
        {
            _context = context;
        }

       


        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound();
            }

            
            foreach (var product in products)
            {
                var imageResult = GetImage(product.Image);

                if (imageResult is FileStreamResult fileStreamResult)
                {
   
                    fileStreamResult.FileStream.Close();

                
                    product.Image = GenerateImageLink(product.Image);
                }

            }

            return await _context.Products.ToListAsync();
        }


        [HttpGet("GetImage/{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            var imagePath = Path.Combine("D:\\NetCore7\\CartWebApi\\", imageName);

            if (System.IO.File.Exists(imagePath))
            {
                var imageStream = System.IO.File.OpenRead(imagePath);
                return File(imageStream, "image/jpeg");
            }
            else
            {
                return NotFound();
            }
        }

        private string GenerateImageLink(string imageName)
        {
            
            return $"http://localhost:5030/api/Products/GetImage/{Uri.EscapeDataString(imageName)}";
        }

        // GET: api/Products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
          if (_context.Products == null)
          {
              return NotFound();
          }
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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


        
        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        
        public async Task<ActionResult<Product>> PostProduct([FromForm]ProductDTO productDTO)
        {
            if (productDTO.Image == null || productDTO.Image.Length == 0)
            {
                return BadRequest("Image is required.");
            }

          

            var staticFilesFolder = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles");

            if (!Directory.Exists(staticFilesFolder))
            {
                Directory.CreateDirectory(staticFilesFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(productDTO.Image.FileName);
            var filePath = Path.Combine(staticFilesFolder, uniqueFileName);
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await productDTO.Image.CopyToAsync(stream);
            }

            var product = new Product
            {
                Name = productDTO.Name,
                Description = productDTO.Description,
                Slug = productDTO.Slug,
                Price = productDTO.Price,
                Quantity = productDTO.Quantity,
                CategoryId = productDTO.CategoryId,
                Image = relativePath,
                DateAdded = DateTime.Now
            };

           
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await productDTO.Image.CopyToAsync(stream);
            }

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        [HttpGet("{categorySlug}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategorySlug(string categorySlug)
        {
            if (!_context.Products.Any())
            {
                return NotFound();
            }

            var category = await _context.Categories
                .SingleOrDefaultAsync(c => c.Slug == categorySlug );

            if (category == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Where(p => p.CategoryId == category.Id)
                .ToListAsync();

            return products;
        }



        [HttpGet("{categorySlug}/{productSlug}")]
        public async Task<ActionResult<Product>> GetProductBySlug(string categorySlug, string productSlug)
        {
            if (!_context.Products.Any())
            {
                return NotFound();
            }

            var product = await _context.Products
                .SingleOrDefaultAsync(p => p.Category.Slug == categorySlug && p.Slug == productSlug);

            if (product == null)
            {
                return NotFound();
            }

          
            var imageResult = GetImage(product.Image);

            if (imageResult is FileStreamResult fileStreamResult)
            {

               fileStreamResult.FileStream.Close();

                product.Image = GenerateImageLink(product.Image);
            }

            return product;
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
