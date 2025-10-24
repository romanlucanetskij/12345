using System.Xml.Schema;
using Microsoft.AspNetCore.Mvc;
using MvcXmlConfigApp.Models;
using MvcXmlConfigApp.Services;

namespace MvcXmlConfigApp.Controllers;

public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetSettingsAsync(cancellationToken);
        return View(settings);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var settings = await _settingsService.GetSettingsAsync(cancellationToken);
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppSettings settings, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(settings);
        }

        try
        {
            await _settingsService.UpdateSettingsAsync(settings, cancellationToken);
            TempData["StatusMessage"] = "Settings saved successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (XmlSchemaValidationException ex)
        {
            ModelState.AddModelError(string.Empty, $"XML validation failed: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Unable to save settings: {ex.Message}");
        }

        return View(settings);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
