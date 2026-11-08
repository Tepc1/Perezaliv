using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRETAIL.Data;
using PCRETAIL.Models;
using System.Globalization;
using System.Security.Claims;

namespace PCRETAIL.Controllers
{
    [Authorize]
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Client")]
        public IActionResult Client()
        {
            var userId = GetCurrentUserId();
            var services = _context.Services
                .Include(s => s.Master)
                .Where(s => s.ClientId == userId)
                .ToList();
            return View(services);
        }

        [Authorize(Roles = "Client")]
        public IActionResult CreateService() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CreateService(Service model)
        {
            // Полностью очищаем ModelState, чтобы не мешали скрытые ошибки биндинга
            ModelState.Clear();

            // Явная обработка цены, чтобы не ломаться из‑за запятой/точки
            var priceRaw = Request.Form["Price"].ToString();
            if (!string.IsNullOrWhiteSpace(priceRaw))
            {
                priceRaw = priceRaw.Trim();

                decimal parsedPrice;
                var ru = CultureInfo.GetCultureInfo("ru-RU");

                var ok =
                    decimal.TryParse(priceRaw, NumberStyles.Any, ru, out parsedPrice) ||
                    decimal.TryParse(priceRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedPrice);

                if (ok)
                {
                    model.Price = parsedPrice;
                }
                else
                {
                    ModelState.AddModelError(nameof(Service.Price), "Введите корректную цену (например 1000,50).");
                }
            }

            // Ручная проверка обязательных полей
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError(nameof(Service.Title), "Введите название услуги");
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                ModelState.AddModelError(nameof(Service.Description), "Введите описание услуги");
            }

            if (ModelState.IsValid)
            {
                model.ClientId = GetCurrentUserId();
                model.Status = "Pending";
                model.CreatedDate = DateTime.Now;

                // Автоматически назначаем мастера: берём любого пользователя с ролью Master
                var masterRole = await _context.Roles
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.Name == "Master");

                var master = masterRole?.Users.FirstOrDefault();
                if (master != null)
                {
                    model.MasterId = master.Id;
                }

                _context.Services.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Client));
            }

            return View(model);
        }

        [Authorize(Roles = "Client")]
        public IActionResult EditService(int id)
        {
            var userId = GetCurrentUserId();
            var service = _context.Services
                .FirstOrDefault(s => s.Id == id && s.ClientId == userId);

            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> EditService(int id, Service model)
        {
            var userId = GetCurrentUserId();
            var existing = _context.Services
                .FirstOrDefault(s => s.Id == id && s.ClientId == userId);

            if (existing == null)
                return NotFound();

            ModelState.Clear();

            var priceRaw = Request.Form["Price"].ToString();
            if (!string.IsNullOrWhiteSpace(priceRaw))
            {
                priceRaw = priceRaw.Trim();

                decimal parsedPrice;
                var ru = CultureInfo.GetCultureInfo("ru-RU");

                var ok =
                    decimal.TryParse(priceRaw, NumberStyles.Any, ru, out parsedPrice) ||
                    decimal.TryParse(priceRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedPrice);

                if (ok)
                {
                    existing.Price = parsedPrice;
                }
                else
                {
                    ModelState.AddModelError(nameof(Service.Price), "Введите корректную цену (например 1000,50).");
                }
            }
            else
            {
                existing.Price = null;
            }

            if (string.IsNullOrWhiteSpace(model.Title))
                ModelState.AddModelError(nameof(Service.Title), "Введите название услуги");

            if (string.IsNullOrWhiteSpace(model.Description))
                ModelState.AddModelError(nameof(Service.Description), "Введите описание услуги");

            if (!ModelState.IsValid)
                return View(existing);

            existing.Title = model.Title;
            existing.Description = model.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Client));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var userId = GetCurrentUserId();
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == id && s.ClientId == userId);

            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Client));
        }

        [Authorize(Roles = "Master")]
        public IActionResult Master()
        {
            var userId = GetCurrentUserId();
            var services = _context.Services
                .Include(s => s.Client)
                .Where(s => s.MasterId == userId)
                .ToList();
            return View(services);
        }

        [Authorize(Roles = "Master")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null && service.MasterId == GetCurrentUserId())
            {
                service.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Master));
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue("UserId"));
        }
    }
}