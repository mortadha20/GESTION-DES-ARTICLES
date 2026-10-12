using GESTION_DES_ARTICLES.Models;
using GESTION_DES_ARTICLES.Models.Help;
using GESTION_DES_ARTICLES.Models.Repositories;
using GESTION_DES_ARTICLES.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GESTION_DES_ARTICLES.Controllers
{
    public class PanierController : Controller
    {
        readonly IProductRepository productRepository;
        readonly IOrderRepository orderRepository;
        readonly ICategorieRepository categRepository;
        private readonly UserManager<IdentityUser> userManager;

        public PanierController(IProductRepository productRepository,
                                IOrderRepository orderRepository,
                                ICategorieRepository categRepository,
                                UserManager<IdentityUser> userManager)
        {
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.categRepository = categRepository;
            this.userManager = userManager;
        }

        public ActionResult Index()
        {
            ViewData["Categories"] = categRepository.GetAll();
            ViewBag.Liste = ListeCart.Instance.Items;
            ViewBag.total = ListeCart.Instance.GetSubTotal();
            return View();
        }

        public ActionResult AddProduct(int id)
        {
            Product? pp = productRepository.GetById(id);
            if (pp == null) return NotFound();

            ListeCart.Instance.AddItem(pp);
            ViewData["Categories"] = categRepository.GetAll();
            ViewBag.Liste = ListeCart.Instance.Items;
            ViewBag.total = ListeCart.Instance.GetSubTotal();
            return View();
        }

        [HttpPost]
        public ActionResult PlusProduct(int id)
        {
            Product? pp = productRepository.GetById(id);
            if (pp == null) return Json(new { ct = 0 });

            ListeCart.Instance.AddItem(pp);
            Item? trouve = ListeCart.Instance.Items.FirstOrDefault(a => a.Prod.ProductId == pp.ProductId);

            return Json(new
            {
                ct = 1,
                Total = ListeCart.Instance.GetSubTotal(),
                Quatite = trouve?.quantite ?? 0,
                TotalRow = trouve?.TotalPrice ?? 0
            });
        }

        [HttpPost]
        public ActionResult MinusProduct(int id)
        {
            Product? pp = productRepository.GetById(id);
            if (pp == null) return Json(new { ct = 0 });

            ListeCart.Instance.SetLessOneItem(pp);
            Item? trouve = ListeCart.Instance.Items.FirstOrDefault(a => a.Prod.ProductId == pp.ProductId);

            if (trouve != null)
            {
                return Json(new
                {
                    ct = 1,
                    Total = ListeCart.Instance.GetSubTotal(),
                    Quatite = trouve.quantite,
                    TotalRow = trouve.TotalPrice
                });
            }
            return Json(new { ct = 0 });
        }

        [HttpPost]
        public ActionResult RemoveProduct(int id)
        {
            Product? pp = productRepository.GetById(id);
            if (pp != null) ListeCart.Instance.RemoveItem(pp);
            return Json(new { Total = ListeCart.Instance.GetSubTotal() });
        }

        // ---------- Commande ----------
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            ViewData["Categories"] = categRepository.GetAll();

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });

            var cartItems = ListeCart.Instance.Items.ToList();
            if (!cartItems.Any())
                return RedirectToAction("Index");

            var viewModel = new OrderViewModel
            {
                CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    ProductName = item.Prod.Name,
                    Quantity = item.quantite,
                    Price = item.Prod.Price
                }).ToList(),
                TotalAmount = ListeCart.Instance.GetSubTotal()
            };
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            ViewData["Categories"] = categRepository.GetAll();

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur non authentifié.";
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }

            // On reprend toujours les articles du panier côté serveur
            var cartItems = ListeCart.Instance.Items.ToList();
            model.CartItems = cartItems.Select(item => new CartItemViewModel
            {
                ProductName = item.Prod.Name,
                Quantity = item.quantite,
                Price = item.Prod.Price
            }).ToList();
            model.TotalAmount = ListeCart.Instance.GetSubTotal();

            if (ModelState.IsValid && model.CartItems.Any())
            {
                var order = new Order
                {
                    CustomerName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Address = model.Address,
                    TotalAmount = model.TotalAmount,
                    OrderDate = DateTime.Now,
                    UserId = user.Id,
                    Items = model.CartItems.Select(item => new OrderItem
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                orderRepository.Add(order);
                ListeCart.Instance.Items.Clear();

                TempData["SuccessMessage"] = "Votre commande a été passée avec succès.";
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }

            TempData["ErrorMessage"] = "Une erreur est survenue. Veuillez vérifier les informations.";
            return View(model);
        }

        [Authorize]
        public IActionResult Confirmation(int orderId)
        {
            ViewData["Categories"] = categRepository.GetAll();
            var order = orderRepository.GetById(orderId);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}