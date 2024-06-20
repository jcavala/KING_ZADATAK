
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static string _token;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        _client = _factory.CreateClient();
        
    }

    [Fact]
    public async Task Login_ValidUser_ReturnsToken()
    {
        var userLogin = new { Email = "emily.johnson@x.dummyjson.com", Password = "emilyspass" };
        var content = new StringContent(JsonConvert.SerializeObject(userLogin), Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", responseString.ToLower());
    }

    [Fact]
    public async Task Protected_Endpoint_ReturnsUnauthorizedWithoutToken()
    {
        var response = await _client.GetAsync("/api/auth/protected");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Authorized_Endpoint_Returns_Products()
    {
        var userLogin = new { Email = "emily.johnson@x.dummyjson.com", Password = "emilyspass" };
        var content = new StringContent(JsonConvert.SerializeObject(userLogin), Encoding.UTF8, "application/json");

        var loginResponse = await _client.PostAsync("/api/auth/login", content);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeObject<dynamic>(loginResponseString).accessToken.ToString();
        _token = token;
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.GetAsync("/api/products");

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var products = JArray.Parse(responseString);
        Assert.NotEmpty(products);
        Assert.NotNull(products[0]);

        foreach (var product in products)
        {
            var descriptionLength = product["Description"].ToString().Length;
            Assert.True(descriptionLength > 0 && descriptionLength <= 100);
        }
    }

    [Fact]
    public async void Endpoint_Returns_Requested_Product()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        var response = await _client.GetAsync("/api/products/1");

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var product = JObject.Parse(responseString);
        Assert.NotEmpty(product);
        Assert.True(product.ContainsKey("id"));
        var id = product.GetValue("id").ToString();
        Assert.Equal("1", id);
    }
    [Fact]
    public async void Endpoint_Returns_Products_FilteredBy_Category_and_Price()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.GetAsync("/api/products/beauty/15");

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var products = JArray.Parse(responseString);
        foreach(var product in products) { 
            Assert.Equal("beauty", product["category"].ToString());
            var product_price = Decimal.Parse(product["price"].ToString());
            Assert.True(product_price <= 15);
        }
    }

    [Fact]
    public async void Endpoint_Returns_Product_By_Name()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.GetAsync("/api/products/byName/Red%20Nail%20Polish");

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var product = JObject.Parse(responseString);
        var productTitle = product["title"].ToString();
        Assert.True(productTitle == "Red Nail Polish");
    }
}
