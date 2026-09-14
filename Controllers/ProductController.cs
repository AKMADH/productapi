using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureProductApi.Model;


namespace SecureProductApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private static readonly List<Product> Products =
    [
        new Product
        {
            Id = 1,
            Name = "Hydraulic Seal",
            Price = 25,
            Stock = 100
        },
        new Product
        {
            Id = 2,
            Name = "O-Ring",
            Price = 10,
            Stock = 200
        }
    ];

    [HttpGet]
    //[Authorize]
    [EnableRateLimiting("api")]
    public IActionResult GetProducts()
    {
        return Ok(Products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateProduct(Product product)
    {
        product.Id = Products.Count + 1;

        Products.Add(product);

        return Ok(product);
    }
}