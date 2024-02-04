using BulkyRazorWeb.Data;
using BulkyRazorWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyRazorWeb.Pages.Categories
{
	[BindProperties]
    public class EditModel : PageModel
    {
		private readonly ApplicationDbContext _db;
		public Category Category { get; set; }
		public EditModel(ApplicationDbContext db)
		{
			_db = db;
		}
		public IActionResult OnPost()
		{
            if (ModelState.IsValid)
            {
                _db.Categories.Update(Category);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully!";
                return RedirectToPage("Index");
            }
            return Page();
        }
		public void OnGet(int Id)
        {
            if (Id == 0)
            {
                return;
            }
            Category = _db.Categories.Find(Id);
            if (Category == null)
            {
                return;
            }
            return;
        }
    }
}
