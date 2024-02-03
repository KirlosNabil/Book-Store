using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Category> categoryList = _db.Categories.ToList();
            return View(categoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category newCategory)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(newCategory);
                _db.SaveChanges();
				TempData["DoneSuccessfully"] = "Category created successfully!";
				return RedirectToAction("Index");
            }
            return View();
        }
		public IActionResult Edit(int Id)
		{
            if(Id == 0)
            {
                return NotFound();
            }
            Category categoryToBeEdited = _db.Categories.Find(Id);
            if(categoryToBeEdited == null)
            {
                return NotFound();
            }
			return View(categoryToBeEdited);
		}
		[HttpPost]
		public IActionResult Edit(Category categoryToBeEdited)
		{
			if (ModelState.IsValid)
			{
				_db.Categories.Update(categoryToBeEdited);
				_db.SaveChanges();
				TempData["DoneSuccessfully"] = "Category updated successfully!";
				return RedirectToAction("Index");
			}
			return View();
		}
		public IActionResult Delete(int Id)
		{
			if (Id == 0)
			{
				return NotFound();
			}
			Category categoryToBeEdited = _db.Categories.Find(Id);
			if (categoryToBeEdited == null)
			{
				return NotFound();
			}
			return View(categoryToBeEdited);
		}
		[HttpPost,ActionName("Delete")]
		public IActionResult DeleteCategory(int? Id)
		{
			Category? categoryToBeDeleted = _db.Categories.Find(Id);
			if (categoryToBeDeleted == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(categoryToBeDeleted);
			_db.SaveChanges();
            TempData["DoneSuccessfully"] = "Category deleted successfully!";
			return RedirectToAction("Index");
		}
	}
}
