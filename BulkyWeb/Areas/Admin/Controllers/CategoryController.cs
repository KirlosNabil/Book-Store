using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CategoryController(IUnitOfWork db)
        {
            _UnitOfWork = db;
        }

        public IActionResult Index()
        {
            List<Category> categoryList = _UnitOfWork.Category.GetAll().ToList();
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
                _UnitOfWork.Category.Add(newCategory);
                _UnitOfWork.Save();
                TempData["success"] = "Category created successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int Id)
        {
            if (Id == 0)
            {
                return NotFound();
            }
            Category categoryToBeEdited = _UnitOfWork.Category.Get(u => u.Id == Id);
            if (categoryToBeEdited == null)
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
                _UnitOfWork.Category.Update(categoryToBeEdited);
                _UnitOfWork.Save();
                TempData["success"] = "Category updated successfully!";
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
            Category categoryToBeEdited = _UnitOfWork.Category.Get(u => u.Id == Id);
            if (categoryToBeEdited == null)
            {
                return NotFound();
            }
            return View(categoryToBeEdited);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteCategory(int? Id)
        {
            Category? categoryToBeDeleted = _UnitOfWork.Category.Get(u => u.Id == Id);
            if (categoryToBeDeleted == null)
            {
                return NotFound();
            }
            _UnitOfWork.Category.Remove(categoryToBeDeleted);
            _UnitOfWork.Save();
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}
