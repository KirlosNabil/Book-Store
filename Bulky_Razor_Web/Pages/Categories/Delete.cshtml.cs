using BulkyRazorWeb.Data;
using BulkyRazorWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyRazorWeb.Pages.Categories
{
	[BindProperties]
    public class DeleteModel : PageModel
    {
		private readonly ApplicationDbContext _db;
		public Category Category { get; set; }
		public DeleteModel(ApplicationDbContext db)
		{
			_db = db;
		}
		public IActionResult OnPost()
		{
			_db.Categories.Remove(Category);
			_db.SaveChanges();
            TempData["success"] = "Category deleted successfully!";
            return RedirectToPage("Index");
		}
		public void OnGet(int Id)
        {
			Category = _db.Categories.Find(Id);
		}
    }
}
