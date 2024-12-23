using CRUD2.Data;
using CRUD2.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace CRUD2.Controllers
{
    public class UserController : Controller
    {
        private readonly ProductDbContext userDbContext;
        IWebHostEnvironment webHostEnvironment;

        public UserController(ProductDbContext userDbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.userDbContext = userDbContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var Users = await userDbContext.Product.ToListAsync();
            return View(Users);
        }
        [HttpGet]
        public async Task<IActionResult> Index2()
        {
            var users = await userDbContext.Product.ToListAsync();
            return View(users);
        }
        [HttpGet]
        public async Task<IActionResult> see(Guid Id)
        {

            var user = await userDbContext.Product.FirstOrDefaultAsync(x => x.Id == Id);

            return View(user);
        }
        public async Task<IActionResult> cart(Guid Id)
        {

            var user = await userDbContext.Product.FirstOrDefaultAsync(x => x.Id == Id);

            return View(user);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(AddProductModel addUserRequest)
        {
            string filename = "";
            if (addUserRequest.photo != null && addUserRequest.photo.Length > 0)
            {
                try
                {
                    string uploadFolder = Path.Combine(webHostEnvironment.WebRootPath, "Images");
                    filename = Guid.NewGuid().ToString() + "_" + Path.GetFileName(addUserRequest.photo.FileName);
                    string filePath = Path.Combine(uploadFolder, filename);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await addUserRequest.photo.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"File upload failed: {ex.Message}");
                    return View(addUserRequest);
                }
            }
            else
            {
                ModelState.AddModelError("", "Please upload a valid photo.");
                return View(addUserRequest);
            }

            var userModel = new ProductModel
            {
                Id = Guid.NewGuid(),
                Name = addUserRequest.Name,
                Description = addUserRequest.Description,
                Quantity = addUserRequest.Quantity,
                Price = addUserRequest.Price,
                Image = filename
            };

            await userDbContext.Product.AddAsync(userModel);
            await userDbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid Id)
        {
            var User = await userDbContext.Product.FirstOrDefaultAsync(x => x.Id == Id);
            if (User != null)
            {
                var editModel = new EditProductModel()

                {
                    Id = User.Id,
                    Name = User.Name,
                    Description = User.Description,
                    Quantity = User.Quantity,
                    Price = User.Price,
                };
                return View(editModel);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProductModel model, IFormFile photo)
        {
            var user = await userDbContext.Product.FindAsync(model.Id);
            if (user != null)
            {
                user.Name = model.Name;
                user.Description = model.Description;
                user.Quantity = model.Quantity;
                user.Price = model.Price;

                if (photo != null && photo.Length > 0)
                {
                    try
                    {
                        string uploadFolder = Path.Combine(webHostEnvironment.WebRootPath, "Images");
                        string filename = Guid.NewGuid().ToString() + "_" + Path.GetFileName(photo.FileName);
                        string filePath = Path.Combine(uploadFolder, filename);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(fileStream);
                        }

                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(user.Image))
                        {
                            var oldImagePath = Path.Combine(uploadFolder, user.Image);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        user.Image = filename;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"File upload failed: {ex.Message}");
                        return View(model);
                    }
                }

                await userDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(EditProductModel model)
        {
            var User = await userDbContext.Product.FindAsync(model.Id);
            if (User != null)
            {
                userDbContext.Product.Remove(User);
                await userDbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddToCart()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(ProductModel model)
        {
            try
            {
                // Fetch product details based on the product ID
                var product = await userDbContext.Product.FindAsync(model.Id);

                if (product != null)
                {
                    // Check if the item already exists in the cart
                    var cartItem = await userDbContext.Cart.FirstOrDefaultAsync(c => c.ProductId == product.Id);

                    if (cartItem != null)
                    {
                        // If the item exists, increment the quantity
                        cartItem.Quantity += 1;
                        userDbContext.Cart.Update(cartItem);
                    }
                    else
                    {
                        // Create a new cart item if it doesn't exist
                        cartItem = new CartModel
                        {
                            ProductId = product.Id,
                            Quantity = 1,
                            Product = product
                        };
                        await userDbContext.Cart.AddAsync(cartItem);
                    }

                    await userDbContext.SaveChangesAsync();

                    TempData["ItemAdded"] = true; // Set TempData to show alert
                    return RedirectToAction("Index2", "User");
                }
                else
                {
                    ModelState.AddModelError("", "Product not found.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error adding product to cart: {ex.Message}");
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> ViewCart()
        {
            List<CartModel> cartItems = userDbContext.Cart.ToList();

            // List to store product details
            List<ProductModel> products = new List<ProductModel>();

            // Fetch product details for each cart item
            foreach (var cartItem in cartItems)
            {
                // Retrieve product details based on product ID
                ProductModel? product = await userDbContext.Product.FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                // If product exists, add it to the list
                if (product != null)
                {
                    product.Quantity = 1;
                    products.Add(product);
                }
            }

            // Pass the list of product details to the view
            return View(products);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity(Guid productId, int quantity)
        {
            try
            {
                var cartItem = await userDbContext.Cart.FirstOrDefaultAsync(c => c.ProductId == productId);

                if (cartItem != null)
                {
                    // Update the quantity of the cart item
                    cartItem.Quantity = quantity;

                    // Save changes to the database
                    await userDbContext.SaveChangesAsync();
                }

                // Redirect back to the cart view
                return RedirectToAction("ViewCart");
            }
            catch (Exception ex)
            {
                // Handle errors
                ModelState.AddModelError("", $"Error updating cart item quantity: {ex.Message}");
                return RedirectToAction("ViewCart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCart(Guid productId)
        {
            try
            {
                // Find the cart item by product ID
                var cartItem = await userDbContext.Cart.FirstOrDefaultAsync(c => c.ProductId == productId);

                if (cartItem != null)
                {
                    userDbContext.Cart.Remove(cartItem);
                    await userDbContext.SaveChangesAsync();
                }

                return RedirectToAction("ViewCart");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting cart item: {ex.Message}");
                return RedirectToAction("ViewCart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuyNow()
        {
            // Assuming you have a way to identify the current user
            // You might have to pass the user ID in a real application
            // Here, we're just assuming a single user for simplicity

            try
            {
                // Fetch cart items for the current user
                var cartItems = await userDbContext.Cart
                    .Include(c => c.Product)
                    .ToListAsync();

                // Create orders from the cart items
                foreach (var cartItem in cartItems)
                {
                    var order = new OrderModel
                    {
                        CartId = cartItem.CartId,
                        Quantity = cartItem.Quantity,
                        Price = (decimal)cartItem.Product.Price,
                    };

                    userDbContext.orders.Add(order);
                }

                // Save orders to the database
                await userDbContext.SaveChangesAsync();

                // Clear the cart after creating orders
                userDbContext.Cart.RemoveRange(cartItems);
                await userDbContext.SaveChangesAsync();

                return RedirectToAction("BuyDetails");
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                ModelState.AddModelError("", $"Error processing order: {ex.Message}");
                return RedirectToAction("ViewCart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuyDetails()
        {
            // Fetch orders for the current user
            var orders = await userDbContext.orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.Product)
                .ToListAsync();

            // Map orders to a view model if necessary
            // For simplicity, we will pass the orders directly to the view

            return View(orders);
        }
    }
}
