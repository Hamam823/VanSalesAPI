using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanSalesAPI.Data;
using VanSalesAPI.DTOs;
using VanSalesAPI.Models;

namespace VanSalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // 📦 GET all products
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var products = await _context.Products
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Cost = p.Cost,
                    Stock = p.Stock
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(
                true,
                "Products loaded successfully",
                products
            ));
        }

        // 📦 GET by id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Cost = p.Cost,
                    Stock = p.Stock
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new ApiResponse<string>(
                    false,
                    "Product not found",
                    null
                ));

            return Ok(new ApiResponse<ProductDto>(
                true,
                "Product loaded successfully",
                product
            ));
        }

        // ➕ CREATE
        [HttpPost]
        public async Task<ActionResult> Create(ProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Cost = dto.Cost,
                Stock = dto.Stock
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>(
                true,
                "Product created successfully",
                new { product.Id }
            ));
        }

        // ✏️ UPDATE
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, ProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new ApiResponse<string>(
                    false,
                    "Product not found",
                    null
                ));

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Cost = dto.Cost;
            product.Stock = dto.Stock;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>(
                true,
                "Product updated successfully",
                new { product.Id }
            ));
        }

        // ❌ DELETE
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound(new ApiResponse<string>(
                    false,
                    "Product not found",
                    null
                ));

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>(
                true,
                "Product deleted successfully",
                null
            ));
        }
    }
}