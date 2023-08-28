using Microsoft.AspNetCore.Mvc;
using Redis.API.Models;
using Redis.API.Repositories;
using Redis.API.Services;

namespace Redis.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Route("getAll")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _productService.GetAsync());
        }

        [HttpGet]
        [Route("getById")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _productService.GetByIdAsync(id));
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            return Created(string.Empty, await _productService.CreateAsync(product));
        }
    }
}
