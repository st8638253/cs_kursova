
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace OnlineShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<ProductController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }


        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }


public async Task<IActionResult> Details(int? id)
{
    if (id == null)
    {
        return NotFound();
    }
    var product = await _context.Products
        .FirstOrDefaultAsync(m => m.Id == id);
    if (product == null)
    {
        return NotFound();
    }
    return View(product);
}

[Authorize(Roles = "Admin")]
public IActionResult Create()
{
    return View();
}


[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,ImageFile")] Product product)
{
    _logger.LogInformation("Attempting to create a new product.");

    _logger.LogInformation($"Product Name: {product.Name}, Price: {product.Price}");


    if (product.ImageFile == null)
    {
        _logger.LogWarning("ImageFile is NULL. No file was uploaded or bound.");
    }
    else
    {
        _logger.LogInformation($"ImageFile detected. File Name: {product.ImageFile.FileName}, Size: {product.ImageFile.Length} bytes.");
    }



    if (product.ImageFile != null)
    {
        try
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
            string path = Path.Combine(wwwRootPath, "images", fileName);

            _logger.LogInformation($"Attempting to save image to: {path}");


            if (!Directory.Exists(Path.Combine(wwwRootPath, "images")))
            {
                _logger.LogInformation($"Creating 'images' directory at: {Path.Combine(wwwRootPath, "images")}");
                Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));
            }

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await product.ImageFile.CopyToAsync(fileStream);
            }
            product.ImageUrl = "/images/" + fileName;
            _logger.LogInformation($"Image successfully saved. ImageUrl set to: {product.ImageUrl}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during image file upload.");



            ModelState.AddModelError("", $"Помилка завантаження зображення: {ex.Message}");
            return View(product);
        }
    }
    else
    {

        product.ImageUrl = "/images/default.png";
        _logger.LogInformation("No image file provided, setting default image URL.");
    }

    try
    {
        _context.Add(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Product '{product.Name}' successfully added to database with Id: {product.Id}");
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred during product database save.");

        ModelState.AddModelError("", $"Помилка збереження товару: {ex.Message}");
        return View(product);
    }
}


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,ImageFile")] Product product)
{
    _logger.LogInformation($"[ProductController.Edit (POST)] Attempting to edit product with Id: {id}");
    _logger.LogInformation($"[ProductController.Edit (POST)] Received product Name: {product.Name}, Description: {product.Description}, Price: {product.Price}");

    if (id != product.Id)
    {
        _logger.LogWarning($"[ProductController.Edit (POST)] Product ID mismatch: route ID {id}, model ID {product.Id}");
        return NotFound();
    }

    if (!ModelState.IsValid)
    {
        _logger.LogWarning("[ProductController.Edit (POST)] ModelState is NOT valid. Validation errors found:");
        foreach (var modelStateEntry in ModelState)
        {
            if (modelStateEntry.Value.Errors.Any())
            {
                _logger.LogError($"[ProductController.Edit (POST)] Field '{modelStateEntry.Key}':");
                foreach (var error in modelStateEntry.Value.Errors)
                {
                    _logger.LogError($"[ProductController.Edit (POST)] - Error: {error.ErrorMessage}");
                }
            }
        }
        return View(product);
    }

    try
    {
        _logger.LogInformation("[ProductController.Edit (POST)] ModelState is valid. Proceeding with update logic.");

        
        if (product.ImageFile != null)
        {
            
            var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
            if (oldProduct != null && !string.IsNullOrEmpty(oldProduct.ImageUrl) && oldProduct.ImageUrl != "/images/default.png")
            {
                string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, oldProduct.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                    _logger.LogInformation($"[ProductController.Edit (POST)] Deleted old image: {oldImagePath}");
                }
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
            string path = Path.Combine(wwwRootPath, "images", fileName);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await product.ImageFile.CopyToAsync(fileStream);
            }
            product.ImageUrl = "/images/" + fileName;
            _logger.LogInformation($"[ProductController.Edit (POST)] New image saved: {product.ImageUrl}");
        }
        
        else 
        {
            
            
            
            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                product.ImageUrl = existingProduct.ImageUrl;
                _logger.LogInformation($"[ProductController.Edit (POST)] No new image. Retained old ImageUrl: {product.ImageUrl}");
            }
            else
            {
                
                
                
                _logger.LogWarning($"[ProductController.Edit (POST)] Existing product with Id {product.Id} not found when trying to retain ImageUrl.");
            }
        }
        

        _context.Update(product); 
        await _context.SaveChangesAsync();
        _logger.LogInformation($"[ProductController.Edit (POST)] Product Id {product.Id} successfully updated in database.");
        return RedirectToAction(nameof(Index));
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogError(ex, $"[ProductController.Edit (POST)] DbUpdateConcurrencyException occurred for product Id {product.Id}");
        if (!ProductExists(product.Id))
        {
            return NotFound();
        }
        else
        {
            throw;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"[ProductController.Edit (POST)] Unexpected error during product update for Id {product.Id}");
        ModelState.AddModelError("", $"Помилка при оновленні товару: {ex.Message}");
        return View(product);
    }
}


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {

                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/default.png")
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}