using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KING_ZADATAK.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        HttpClient _httpClient;
        IMemoryCache _memoryCache;
        ILogger _logger;
        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(3));
        public ProductController(HttpClient httpClient, IMemoryCache memoryCache, ILogger<ProductController> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            string response = "";
            if (!_memoryCache.TryGetValue("products", out string cacheValue))
            {
                var json = await getProducts($"https://dummyjson.com/products");
                response = ReduceJsonObjects(json);
                _memoryCache.Set("products", response, cacheEntryOptions);
                _logger.LogInformation("products cache refreshed");
            }
            else
            {
                _logger.LogInformation("products retrieved from cache");
                response = ReduceJsonObjects(cacheValue);
                
            }
            _logger.LogInformation("api/products endpoint is returning list of products : " + response);
            return Ok(response);

        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            string response = "";
            if (_memoryCache.TryGetValue($"product:{id}", out string cacheValue))
            {
                response = cacheValue;
                _logger.LogInformation($"product:{id} retrieved from cache");
            }
            else
            {
                response = await getProducts($"https://dummyjson.com/products/{id}");
                _memoryCache.Set($"product:{id}", response, cacheEntryOptions);
                _logger.LogInformation($"product:{id} cache refreshed");
            }
            _logger.LogInformation($"api/products - argument : {id} endpoint is returning one product : " + response);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("{category}/{price}")]
        public async Task<IActionResult> GetProducts(string category, decimal price)
        {
            string response = "";
            if (_memoryCache.TryGetValue($"products:{category}:{price}", out string cacheValue))
            {
                response = cacheValue;
                _logger.LogInformation($"products:{category}:{price} retrieved from cache");
            }
            else
            {
                var json = await getProducts($"https://dummyjson.com/products/category/{category}");
                response = FilterProductsByPrice(json, price);
                _memoryCache.Set($"products:{category}:{price}", response, cacheEntryOptions);
                _logger.LogInformation($"products:{category}:{price} cache refreshed");
            }
            _logger.LogInformation($"api/products - arguments: {category},{price} endpoint is returning filtered list of products : " + response);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("byName/{name}")]
        public async Task<IActionResult> GetProducts(string name)
        {
            string response = "";
            if (_memoryCache.TryGetValue($"products:{name}", out string cacheValue))
            {
                response = cacheValue;
                _logger.LogInformation($"products:{name} retrieved from cache");
            }
            else
            {
                var json = await getProducts("https://dummyjson.com/products");
                response = FilterProductsByName(json, name);
                _memoryCache.Set($"products:{name}", response, cacheEntryOptions);
                _logger.LogInformation($"products:{name} cache refreshed");
            }
            _logger.LogInformation($"api/products - argument: {name} endpoint is returning one product : " + response);

            return Ok(response);
        }

        async Task<string> getProducts(string url)
        {
            var json =await _httpClient.GetStringAsync(url);
            var obj = System.Text.Json.JsonDocument.Parse(json);

            if(obj.RootElement.TryGetProperty("products",out var products))
            {
                return products.ToString();
            }
            
            return obj.RootElement.ToString();
            
            
        }
        

        string ReduceJsonObjects(string json)
        {
            List<Product> products = new List<Product>();
            JArray jArray = JArray.Parse(json);
            foreach (var item in jArray) {
                string description;
                if (item["description"].ToString().Length <= 100) { description = item["description"].ToString(); }
                else { description = item["description"].ToString().Substring(0, 100); }
                 
                products.Add(new Product
                {
                    Title = item["title"].ToString(),
                    Price = Decimal.Parse(item["price"].ToString()),
                    Thumbnail = item["thumbnail"].ToString(),
                    Description = description

                }); }

                return JsonConvert.SerializeObject(products);

        }

        string FilterProductsByPrice(string json, decimal priceThreshold)
        {
            JArray jsonArray = JArray.Parse(json);
            var filteredProducts = jsonArray.Where(item => (decimal)item["price"] <= priceThreshold);
            JArray filteredJsonArray = new JArray(filteredProducts);
            return filteredJsonArray.ToString(Formatting.None);
        }
        string FilterProductsByName(string json, string name)
        {
            JArray jsonArray = JArray.Parse(json);
            var product = jsonArray.Where(item => item["title"].ToString().Contains(name)).FirstOrDefault().ToString();
            return product;
        }
    }

    class Product
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Thumbnail { get; set; }
    }
}
