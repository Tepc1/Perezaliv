using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRETAIL.Data;
using PCRETAIL.Models;
using System.IO;
using System.Security.Claims;

namespace PCRETAIL.Controllers
{
    [Authorize]
    public class ConfiguratorController : Controller
    {
        private const string ImagesRootFolderName = "PCRETAILIMAGES";
        private const string ConfigurationsFolderName = "CONFIGURATIONS";
        private const string ComponentsFolderName = "COMPONENTS";
        private const string ConfigurationImagesRequestPath = "/configimages";
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

        public ConfiguratorController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var configs = await _context.PcConfigurations
                .Include(c => c.Components)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            ViewBag.ConfigurationImageUrls = configs.ToDictionary(
                config => config.Id,
                config => GetConfigurationImageUrl(config.ImagePath));

            return View(configs);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Components = await _context.Components.ToListAsync();
            ViewBag.ImageOptions = GetConfigurationImages();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PcConfiguration model, int[] selectedComponents)
        {
            model.ImagePath = NormalizeImageFileName(model.ImagePath);
            model.Status = NormalizeStatus(model.Status);

            if (ModelState.IsValid)
            {
                model.UserId = GetCurrentUserId();
                model.CreatedDate = DateTime.Now;

                if (selectedComponents != null && selectedComponents.Any())
                {
                    var components = await _context.Components
                        .Where(c => selectedComponents.Contains(c.Id))
                        .ToListAsync();

                    if (HasDuplicateComponentTypes(components))
                    {
                        ModelState.AddModelError("", "Можно выбрать только одно комплектующее каждого типа.");
                    }
                    else
                    {
                        foreach (var component in components)
                        {
                            model.Components.Add(component);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Выберите хотя бы один компонент.");
                }

                if (ModelState.IsValid)
                {
                    _context.PcConfigurations.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Components = await _context.Components.ToListAsync();
            ViewBag.ImageOptions = GetConfigurationImages();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var config = await _context.PcConfigurations
                .Include(c => c.Components)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (config == null)
            {
                return NotFound();
            }

            ViewBag.Components = await _context.Components.ToListAsync();
            ViewBag.ImageOptions = GetConfigurationImages();
            ViewBag.CurrentConfigurationImageUrl = GetConfigurationImageUrl(config.ImagePath);
            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PcConfiguration model, int[] selectedComponents)
        {
            var userId = GetCurrentUserId();
            var existing = await _context.PcConfigurations
                .Include(c => c.Components)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Components = await _context.Components.ToListAsync();
                ViewBag.ImageOptions = GetConfigurationImages();
                ViewBag.CurrentConfigurationImageUrl = GetConfigurationImageUrl(existing.ImagePath);
                return View(existing);
            }

            existing.Name = model.Name;
            existing.Category = model.Category;
            existing.Status = NormalizeStatus(model.Status);
            existing.ImagePath = NormalizeImageFileName(model.ImagePath);

            existing.Components.Clear();
            if (selectedComponents != null && selectedComponents.Any())
            {
                var components = await _context.Components
                    .Where(c => selectedComponents.Contains(c.Id))
                    .ToListAsync();

                if (HasDuplicateComponentTypes(components))
                {
                    ModelState.AddModelError("", "Можно выбрать только одно комплектующее каждого типа.");
                    ViewBag.Components = await _context.Components.ToListAsync();
                    ViewBag.ImageOptions = GetConfigurationImages();
                    ViewBag.CurrentConfigurationImageUrl = GetConfigurationImageUrl(existing.ImagePath);
                    return View(existing);
                }

                foreach (var component in components)
                {
                    existing.Components.Add(component);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private List<string> GetConfigurationImages()
        {
            var configPath = GetConfigurationImagesDirectoryPath();
            Directory.CreateDirectory(configPath);

            return Directory.GetFiles(configPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => AllowedImageExtensions.Contains(Path.GetExtension(file)))
                .Select(file => Path.GetFileName(file)!)
                .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public string? GetConfigurationImageUrl(string? imageName)
        {
            return BuildImageUrl(
                ConfigurationImagesRequestPath,
                GetConfigurationImagesDirectoryPath(),
                imageName);
        }

        public string? GetComponentImageUrl(string? imageName)
        {
            return BuildImageUrl(
                ComponentImagesRequestPath,
                GetComponentImagesDirectoryPath(),
                imageName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var config = await _context.PcConfigurations
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (config != null)
            {
                _context.PcConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var config = await _context.PcConfigurations
                .Include(c => c.Components)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (config == null)
            {
                return NotFound();
            }

            ViewBag.TotalPrice = (config.Components?.Sum(c => c.Price)) ?? 0m;
            ViewBag.ConfigurationImageUrl = GetConfigurationImageUrl(config.ImagePath);

            var componentImages = new Dictionary<int, string>();
            foreach (var component in config.Components ?? Enumerable.Empty<Component>())
            {
                var imageUrl = GetComponentImageUrl(component.ImagePath);
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    componentImages[component.Id] = imageUrl;
                }
            }

            ViewBag.ComponentImages = componentImages;
            return View(config);
        }

        private static bool HasDuplicateComponentTypes(IEnumerable<Component> components)
        {
            return components
                .GroupBy(component => component.Type, StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1);
        }

        private int GetCurrentUserId()
        {
            var claim = User?.FindFirstValue("UserId");
            if (int.TryParse(claim, out var id))
            {
                return id;
            }

            throw new InvalidOperationException("User ID claim not found or invalid");
        }

        private static string GetConfigurationImagesDirectoryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                ImagesRootFolderName,
                ConfigurationsFolderName);
        }

        private static string GetComponentImagesDirectoryPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                ImagesRootFolderName,
                ComponentsFolderName);
        }

        private static string? NormalizeImageFileName(string? imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return null;
            }

            return Path.GetFileName(imageName.Trim());
        }

        private static string NormalizeStatus(string? status)
        {
            return status switch
            {
                "InProgress" => "InProgress",
                "Completed" => "Completed",
                _ => "Pending"
            };
        }

        private static string? BuildImageUrl(string requestPath, string directoryPath, string? imageName)
        {
            var fileName = NormalizeImageFileName(imageName);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            if (!System.IO.File.Exists(Path.Combine(directoryPath, fileName)))
            {
                return null;
            }

            return $"{requestPath}/{Uri.EscapeDataString(fileName)}";
        }
    }
}
