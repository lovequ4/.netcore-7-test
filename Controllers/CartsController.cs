using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CartWebApi.Data;
using CartWebApi.DTO;

namespace CartWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public CartsController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Carts
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        //{
        //  if (_context.Carts == null)
        //  {
        //      return NotFound();
        //  }
        //    return await _context.Carts.ToListAsync();
        //}

        // GET: api/Carts/userId
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<Cart>>> GetCartByUser(string userId)
        {
            if (!_context.Carts.Any())
            {
                return NotFound();
            }

            var userCarts = await _context.Carts
                .Include(c => c.user)
                .Include(c => c.product)
                .Where(c => c.UserId == userId)
                .ToListAsync();


            if (userCarts == null)
            {
                return NotFound();
            }

            foreach (var userCart in userCarts)
            {
                userCart.user = null;  // 不回傳 User 的其他資訊
            } 
            return userCarts;
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(int id, Cart cart)
        {
            if (id != cart.Id)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
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

        // POST: api/Carts/userId
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cart>> PostCart(CartDTO cartDTO)
        {
            var product = await _context.Products
                .SingleOrDefaultAsync(p => p.Id == cartDTO.ProductId);

            if (product == null) 
            {
                return NotFound();
            }

            var cart = new Cart
            {
                UserId = cartDTO.UserId,
                ProductId = cartDTO.ProductId,
                Quantity = cartDTO.Quantity,
                Price = cartDTO.Quantity * product.Price
            };

            //check addToCart quantity > product.quantity
            if (cart.Quantity > product.Quantity)
            {
                return BadRequest(new { Error = "Please choose a valid quantity." });
            }

            var existsCart = await _context.Carts
                .Where(c => c.UserId == cart.UserId)
                .SingleOrDefaultAsync(c => c.ProductId == cart.ProductId);

            //check cart
            if (existsCart != null) 
            {
                existsCart.Quantity += cart.Quantity;
                
                //check  cart quantity > product quantity
                if(existsCart.Quantity > product.Quantity)
                {
                    existsCart.Quantity = product.Quantity;
                    existsCart.Price = existsCart.Quantity * product.Price;

                    await _context.SaveChangesAsync();

                    return new ObjectResult(new { Message = "The number of shopping carts exceeds the number of products, use the highest number of products as your quantity." })
                    {
                        StatusCode = 400 
                    };
                    
                }

                existsCart.Price = existsCart.Quantity * product.Price;
            }
            else
            {
                _context.Carts.Add(cart);
            }

            await _context.SaveChangesAsync();

            return StatusCode(201, cart);
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            if (_context.Carts == null)
            {
                return NotFound();
            }
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int id)
        {
            return (_context.Carts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
