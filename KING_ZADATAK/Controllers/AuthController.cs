using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Security.Claims;


namespace KING_ZADATAK.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        HttpClient _httpClient;
        IMemoryCache _memoryCache;
        
        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(3));
        public AuthController(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            
        }
        [HttpPost("login")]
        public async Task<IResult> Login([FromBody] UserLogin userLogin)
        {
            JArray users;
            if (_memoryCache.TryGetValue($"users", out JArray cacheValue))
            {
                users = cacheValue;
            }
            else
            {
                var json = await _httpClient.GetStringAsync("https://dummyjson.com/users");
                var obj = System.Text.Json.JsonDocument.Parse(json);
                var users_json = obj.RootElement.GetProperty("users");
                users = JArray.Parse(users_json.ToString());
            }
                
            foreach(var user in users)
            {
                if (userLogin.Email == user["email"].ToString() && userLogin.Password == user["password"].ToString())
                {
                    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Name, userLogin.Email) }, "my_scheme"));

                    return Results.SignIn(claimsPrincipal, authenticationScheme: "my_scheme");
                }
            }

            return Results.Unauthorized();
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            return Ok("Zaštićeni endpoint");
        }

        
    }
    public class UserLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
