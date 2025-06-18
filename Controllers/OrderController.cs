
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using OnlineShop.Data;
    using OnlineShop.Models;
    using System.Text.Json;

    namespace OnlineShop.Controllers
    {
        public class OrderController : Controller
        {
            private readonly ApplicationDbContext _context;

              private const string CartSessionKey = "Cart";

            public OrderController(ApplicationDbContext context)
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


            public IActionResult Checkout()
            {
                var cart = GetCart();
                if (!cart.Any())
                {
                    TempData["Message"] = "Ваш кошик порожній. Додайте товари, перш ніж оформлювати замовлення.";
                    return RedirectToAction("Index", "Cart");
                }


                foreach (var item in cart)
                {
                    item.Product = _context.Products.Find(item.ProductId);
                }


                var orderViewModel = new Order
                {
                    OrderItems = cart.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        Product = ci.Product,
                        Quantity = ci.Quantity,
                        Price = ci.Product.Price
                    }).ToList(),
                    TotalAmount = cart.Sum(ci => ci.Product.Price * ci.Quantity)
                };

                return View(orderViewModel);
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> PlaceOrder([Bind("CustomerName,CustomerEmail,CustomerPhone,ShippingAddress")] Order order)
            {
                var cart = GetCart();
                if (!cart.Any())
                {
                    TempData["Message"] = "Ваш кошик порожній. Додайте товари, перш ніж оформлювати замовлення.";
                    return RedirectToAction("Index", "Cart");
                }

                if (ModelState.IsValid)
                {

                    foreach (var item in cart)
                    {
                        item.Product = await _context.Products.FindAsync(item.ProductId);
                        if (item.Product == null)
                        {
                            ModelState.AddModelError("", $"Продукт з ID {item.ProductId} не знайдено.");
                            return View("Checkout", order);
                        }
                    }

                    order.OrderDate = DateTime.Now;
                    order.TotalAmount = cart.Sum(ci => ci.Product.Price * ci.Quantity);


                    foreach (var item in cart)
                    {
                        order.OrderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Product.Price
                        });
                    }

                    _context.Add(order);
                    await _context.SaveChangesAsync();


                    HttpContext.Session.Remove(CartSessionKey);

                    TempData["OrderPlaced"] = $"Ваше замовлення №{order.Id} успішно оформлено!";
                    return RedirectToAction("OrderConfirmation", new { id = order.Id });
                }



                order.OrderItems = cart.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Product = _context.Products.Find(ci.ProductId),
                    Quantity = ci.Quantity,
                    Price = _context.Products.Find(ci.ProductId).Price
                }).ToList();
                return View("Checkout", order);
            }


            public async Task<IActionResult> OrderConfirmation(int id)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
        }
    }