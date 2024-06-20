using KING_ZADATAK.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KING_ZAFATAK_TEST
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            HttpClient httpClient = new HttpClient();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            _controller = new AuthController(httpClient, memoryCache);
        }

        [Fact]
        public void ValidUser_ReturnsToken()
        {
            var result = _controller.Login(new UserLogin { Email = "emily.johnson@x.dummyjson.com", Password = "emilyspass" });

            Assert.NotNull(result);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public void InvalidUser_Returns_Unauthorized()
        {
            var result = _controller.Login(new UserLogin { Email = "invaliduser", Password = "invalidpassword" });

            Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>(result.Result);
        }
    }
}
