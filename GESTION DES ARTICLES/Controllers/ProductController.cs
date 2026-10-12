using GESTION_DES_ARTICLES.Models;
using GESTION_DES_ARTICLES.Models.Repositories;
using GESTION_DES_ARTICLES.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GESTION_DES_ARTICLES.Models;
using GESTION_DES_ARTICLES.Models.Repositories;
using GESTION_DES_ARTICLES.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace GESTION_DES_ARTICLES.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : Controller
    {
        readonly IProductRepository ProductRepository;
        readonly ICategorieRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(IProductRepository ProdRepository,
            ICategorieRepository categRepository,
            IWebHostEnvironment hostingEnvironment)
        {
            ProductRepository = ProdRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
        }
        [AllowAnonymous]
        public IActionResult Index(int? categoryId, int page = 1)
        {
            int pageSize = 4;
            LoadCategories();

            IQueryable<Product> productsQuery = ProductRepository.GetAllProducts();
            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);

            var totalProducts = productsQuery.Count();
            var products = productsQuery
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.CategoryId = categoryId;
            return View(products);
        }
        public ActionResult Details(int id) => View(ProductRepository.GetById(id));

        // GET: Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);
                Product newProduct = new Product
                {
                    Name = model.Name,
                    Price = model.Price,
                    QteStock = model.QteStock,
                    CategoryId = model.CategoryId,
                    Image = uniqueFileName
                };
                ProductRepository.Add(newProduct);
                return RedirectToAction("Details", new { id = newProduct.ProductId });
            }
            return View(model);
        }

        // GET: Edit
        public ActionResult Edit(int id)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            Product product = ProductRepository.GetById(id);
            if (product == null) return NotFound();

            EditViewModel vm = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };
            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            ViewBag.CategoryId = new SelectList(CategRepository.GetAll(), "CategoryId", "CategoryName");
            if (ModelState.IsValid)
            {
                Product product = ProductRepository.GetById(model.ProductId);
                if (product == null) return NotFound();

                product.Name = model.Name;
                product.Price = model.Price;
                product.QteStock = model.QteStock;
                product.CategoryId = model.CategoryId;

                if (model.ImagePath != null)
                {
                    if (model.ExistingImagePath != null)
                    {
                        string oldPath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingImagePath);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }
                    product.Image = ProcessUploadedFile(model);
                }

                Product updated = ProductRepository.Update(product);
                return updated != null ? RedirectToAction("Index") : NotFound();
            }
            return View(model);
        }

        // GET: Delete
        public ActionResult Delete(int id) => View(ProductRepository.GetById(id));

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public ActionResult Search(string val)
        {
            LoadCategories();
            var result = string.IsNullOrWhiteSpace(val)
                ? ProductRepository.GetAll()
                : ProductRepository.FindByName(val);

            // La vue Index utilise ces valeurs : sans elles, la recherche plante
            ViewBag.TotalPages = 1;
            ViewBag.CurrentPage = 1;
            ViewBag.CategoryId = null;
            return View("Index", result);
        }

        [NonAction]
        private string ProcessUploadedFile(CreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.ImagePath != null)
            {
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImagePath.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ImagePath.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
        private void LoadCategories()
        {
            ViewData["Categories"] = CategRepository.GetAll();
        }
    }
}