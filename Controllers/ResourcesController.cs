using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UTHMLibrary.Data;

namespace UTHMLibrary.Controllers;

[Authorize]
public class ResourcesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ResourcesController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // Past Year Papers module
    public async Task<IActionResult> PastYearPapers(string? q)
    {
        var list = _db.Resources.Where(r => r.Category == "PastYearPaper");
        if (!string.IsNullOrWhiteSpace(q))
            list = list.Where(r => r.Title.Contains(q) || r.Faculty.Contains(q) || r.Year.Contains(q));
        ViewBag.Query = q;
        return View(await list.OrderByDescending(r => r.UploadedAt).ToListAsync());
    }

    // E-Books module
    public async Task<IActionResult> EBooks(string? q)
    {
        var list = _db.Resources.Where(r => r.Category == "EBook");
        if (!string.IsNullOrWhiteSpace(q))
            list = list.Where(r => r.Title.Contains(q) || r.Author.Contains(q));
        ViewBag.Query = q;
        return View(await list.OrderByDescending(r => r.UploadedAt).ToListAsync());
    }

    // General Resources module
    public async Task<IActionResult> Index(string? q)
    {
        var list = _db.Resources.Where(r => r.Category == "Resource");
        if (!string.IsNullOrWhiteSpace(q))
            list = list.Where(r => r.Title.Contains(q));
        ViewBag.Query = q;
        return View(await list.OrderByDescending(r => r.UploadedAt).ToListAsync());
    }

    // Undergraduate Project Reports module
    public async Task<IActionResult> ProjectReports(string? q)
    {
        var list = _db.Resources.Where(r => r.Category == "ProjectReport");
        if (!string.IsNullOrWhiteSpace(q))
            list = list.Where(r => r.Title.Contains(q) || r.Author.Contains(q) || r.Year.Contains(q));
        ViewBag.Query = q;
        return View(await list.OrderByDescending(r => r.UploadedAt).ToListAsync());
    }

    public async Task<IActionResult> Download(int id)
    {
        var res = await _db.Resources.FindAsync(id);
        if (res == null) return NotFound();

        var physical = Path.Combine(_env.WebRootPath, res.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(physical))
        {
            TempData["Error"] = "File not found on server.";
            return RedirectToAction("Index");
        }

        res.DownloadCount++;
        await _db.SaveChangesAsync();

        var bytes = await System.IO.File.ReadAllBytesAsync(physical);
        return File(bytes, "application/octet-stream", res.OriginalFileName);
    }
}