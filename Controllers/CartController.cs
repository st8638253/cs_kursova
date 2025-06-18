
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using OnlineShop.Data;
    using OnlineShop.Models;
    using System.Text.Json;

    namespace OnlineShop.Controllers
    {
        public class CartController : Controller
        {
            private readonly ApplicationDbContext _context;
            private const string CartSessionKey = "Cart";

            public CartController(ApplicationDbContext context)
            {
                _context = context;
            }


            private List<CartItem> GetCart()
            {
                string cartJson = HttpContext.Session.GetString(CartSessionKey);
                if (string.IsNullOrEmpty(cartJson))
                {
                    return new List<CartItem>();
                }
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            }


            private void SaveCart(List<CartItem> cart)
            {
                string cartJson = JsonSerializer.Serialize(cart);
                HttpContext.Session.SetString(CartSessionKey, cartJson);
            }

            public IActionResult Index()
            {
                List<CartItem> cart = GetCart();



                 foreach (var item in cart)
{
item.Product = _context.Products.Find(item.ProductId);
}

                return View(cart);
            }

            [HttpPost]
            public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                List<CartItem> cart = GetCart();
                var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = productId,
                        Product = product,
                        Quantity = quantity,
                        SessionId = HttpContext.Session.Id
                    });
                }

                SaveCart(cart);
                return RedirectToAction(nameof(Index));
            }

            [HttpPost]
            public IActionResult UpdateCartItemQuantity(int productId, int quantity)
            {
                List<CartItem> cart = GetCart();
                var itemToUpdate = cart.FirstOrDefault(item => item.ProductId == productId);

                if (itemToUpdate != null)
                {
                    if (quantity > 0)
                    {
                        itemToUpdate.Quantity = quantity;
                    }
                    else
                    {
                        cart.Remove(itemToUpdate);
                    }
                    SaveCart(cart);
                }
                return RedirectToAction(nameof(Index));
            }

            [HttpPost]
            public IActionResult RemoveFromCart(int productId)
            {
                List<CartItem> cart = GetCart();
                var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);

                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);
                    SaveCart(cart);
                }

                  return RedirectToAction(nameof(Index));
}
}
}