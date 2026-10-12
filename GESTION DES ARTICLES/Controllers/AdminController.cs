using GESTION_DES_ARTICLES.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GESTION_DES_ARTICLES.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;

        public AdminController(RoleManager<IdentityRole> roleManager,
                               UserManager<IdentityUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        // ---------- Créer un rôle ----------
        [HttpGet]
        public IActionResult CreateRole() => View();

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole { Name = model.RoleName };
                IdentityResult result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("ListRoles");

                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // ---------- Lister les rôles ----------
        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        // ---------- Modifier un rôle ----------
        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel { Id = role.Id, RoleName = role.Name };

            foreach (var user in userManager.Users.ToList())
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                    model.Users.Add(user.UserName);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                return View("NotFound");
            }

            role.Name = model.RoleName;
            var result = await roleManager.UpdateAsync(role);
            if (result.Succeeded)
                return RedirectToAction("ListRoles");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        // ---------- Supprimer un rôle ----------
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return RedirectToAction("ListRoles");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View("ListRoles", roleManager.Roles);
        }

        // ---------- Ajouter / retirer des utilisateurs d'un rôle ----------
        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            foreach (var user in userManager.Users.ToList())
            {
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsSelected = await userManager.IsInRoleAsync(user, role.Name)
                });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            foreach (var item in model)
            {
                var user = await userManager.FindByIdAsync(item.UserId);
                if (user == null) continue;

                bool inRole = await userManager.IsInRoleAsync(user, role.Name);

                if (item.IsSelected && !inRole)
                    await userManager.AddToRoleAsync(user, role.Name);
                else if (!item.IsSelected && inRole)
                    await userManager.RemoveFromRoleAsync(user, role.Name);
            }
            return RedirectToAction("EditRole", new { Id = roleId });
        }
    }
}