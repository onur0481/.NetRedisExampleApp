using Redis.API.Models;
using RedisExampleApp.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace Redis.API.Repositories
{
    public class ProductRepositoryWithCache : IProductRepository
    {
        private const string productKey = "productCaches";
        private readonly IProductRepository _repository;
        private readonly RedisService _redisService;
        private readonly IDatabase _cacheRepository;
        public ProductRepositoryWithCache(IProductRepository repository, RedisService redisService)
        {
            _repository = repository;
            _redisService = redisService;
            _cacheRepository = _redisService.GetDatabase(2); 
        }

        public async Task<Product> CreateAsync(Product product)
        {
            var newProduct = await _repository.CreateAsync(product);

            if(await _cacheRepository.KeyExistsAsync(productKey))
            {
                await _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize(newProduct));
            }

            return newProduct;
        }

        public async Task<List<Product>> GetAsync()
        {
            if(!await _cacheRepository.KeyExistsAsync(productKey)) return await LoadToCacheFromDbAsync();

            var products = new List<Product>();

            var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);

            foreach(var item in cacheProducts.ToList())
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value);
                products.Add(product);
            }
            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            if (_cacheRepository.KeyExists(productKey))
            {
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
            }
            var products = await LoadToCacheFromDbAsync();

            return products.FirstOrDefault(x => x.Id == id);
        }

        private async Task<List<Product>> LoadToCacheFromDbAsync()
        {
            var products = await _repository.GetAsync();

            products.ForEach(product =>
            {
                _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize(product));
            });

            return products;
        }
    }
}
