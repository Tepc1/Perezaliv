using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRETAIL.Data;
using PCRETAIL.Models;

namespace PCRETAIL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private const string ImagesRootFolderName = "PCRETAILIMAGES";
        private const string ComponentsFolderName = "COMPONENTS";
        private const string ComponentImagesRequestPath = "/componentimages";

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Components()
        {
            var model = await BuildComponentsViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Components(AdminComponentsViewModel model)
        {
            model.ImagePath = NormalizeImageFileName(model.ImagePath);

            if (!ModelState.IsValid)
            {
                model.Components = await LoadComponentsAsync();
                PopulateComponentsPageViewData(model.Components);
                return View(model);
            }

            var component = new Component
            {
                Name = model.Name.Trim(),
                Type = model.Type.Trim(),
                Price = model.Price,
                ImagePath = model.ImagePath,
                CreatedAt = DateTime.Now
            };

            _context.Components.Add(component);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Комплектующее успешно добавлено.";
            return RedirectToAction(nameof(Components));
        }

        [HttpGet]
        public async Task<IActionResult> EditComponent(int id)
        {
            var component = await _context.Components.FindAsync(id);
            if (component == null)
            {
                return NotFound();
            }

            PopulateComponentEditorViewData(component.ImagePath);
            return View(component);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComponent(int id, Component model)
        {
            var existing = await _context.Components.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            model.ImagePath = NormalizeImageFileName(model.ImagePath);

            if (!ModelState.IsValid)
            {
                model.Id = id;
                model.CreatedAt = existing.CreatedAt;
                PopulateComponentEditorViewData(model.ImagePath);
                return View(model);
            }

            existing.Name = model.Name.Trim();
            existing.Type = model.Type.Trim();
            existing.Price = model.Price;
            existing.ImagePath = model.ImagePath;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Карточка комплектующего обновлена.";
            return RedirectToAction(nameof(Components));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            var component = await _context.Components.FindAsync(id);
            if (component == null)
            {
                TempData["ErrorMessage"] = "Комплектующее не найдено.";
                return RedirectToAction(nameof(Components));
            }

            _context.Components.Remove(component);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Комплектующее \"{component.Name}\" удалено.";
            return RedirectToAction(nameof(Components));
        }

        private async Task<AdminComponentsViewModel> BuildComponentsViewModelAsync()
        {
            var components = await LoadComponentsAsync();
            PopulateComponentsPageViewData(components);

            return new AdminComponentsViewModel
            {
                Components = components
            };
        }

        private void PopulateComponentsPageViewData(IEnumerable<Component> components)
        {
            ViewBag.ComponentImageUrls = BuildComponentImageUrls(components);
            ViewBag.ComponentTypes = GetComponentTypes();
            ViewBag.ComponentImageOptions = GetComponentImageOptions();
        }

        private void PopulateComponentEditorViewData(string? currentImagePath)
        {
            ViewBag.ComponentTypes = GetComponentTypes();
            ViewBag.ComponentImageOptions = GetComponentImageOptions();
            ViewBag.CurrentComponentImageUrl = GetComponentImageUrl(currentImagePath);
        }

        private async Task<List<Component>> LoadComponentsAsync()
        {
            return await _context.Components
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        private Dictionary<int, string> BuildComponentImageUrls(IEnumerable<Component> components)
        {
            var result = new Dictionary<int, string>();
            foreach (var component in components)
            {
                var imageUrl = GetComponentImageUrl(component.ImagePath);
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    result[component.Id] = imageUrl;
                }
            }

            return result;
        }

        private static IReadOnlyList<string> GetComponentTypes()
        {
            return new[] { "CPU", "GPU", "RAM", "PSU", "Motherboard", "Case", "Storage", "Cooling", "Other" };
        }

        private static List<string> GetComponentImageOptions()
        {
            var componentsPath = GetComponentImagesDirectoryPath();
            Directory.CreateDirectory(componentsPath);

            return Directory.GetFiles(componentsPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => AllowedImageExtensions.Contains(Path.GetExtension(file)))
                .Select(file => Path.GetFileName(file)!)
                .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string GetComponentImagesDirectoryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                ImagesRootFolderName,
                ComponentsFolderName);
        }

        private static string? GetComponentImageUrl(string? imageName)
        {
            var fileName = NormalizeImageFileName(imageName);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            if (!System.IO.File.Exists(Path.Combine(GetComponentImagesDirectoryPath(), fileName)))
            {
                return null;
            }

            return $"{ComponentImagesRequestPath}/{Uri.EscapeDataString(fileName)}";
        }

        private static string? NormalizeImageFileName(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return null;
            }

            return Path.GetFileName(imageName.Trim());
        }
    }
}
