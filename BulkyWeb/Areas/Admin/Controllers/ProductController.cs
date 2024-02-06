using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public ProductController(IUnitOfWork db)
        {
            _UnitOfWork = db;
        }
        public IActionResult Index()
        {
            List<Product> ProductList = _UnitOfWork.Product.GetAll().ToList();
            return View(ProductList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product newProduct)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.Product.Add(newProduct);
                _UnitOfWork.Save();
                TempData["success"] = "Product created successfully!";
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
            Product ProductToBeEdited = _UnitOfWork.Product.Get(u => u.Id == Id);
            if (ProductToBeEdited == null)
            {
                return NotFound();
            }
            return View(ProductToBeEdited);
        }
        [HttpPost]
        public IActionResult Edit(Product ProductToBeEdited)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.Product.Update(ProductToBeEdited);
                _UnitOfWork.Save();
                TempData["success"] = "Product updated successfully!";
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
            Product ProductToBeEdited = _UnitOfWork.Product.Get(u => u.Id == Id);
            if (ProductToBeEdited == null)
            {
                return NotFound();
            }
            return View(ProductToBeEdited);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteProduct(int? Id)
        {
            Product? ProductToBeDeleted = _UnitOfWork.Product.Get(u => u.Id == Id);
            if (ProductToBeDeleted == null)
            {
                return NotFound();
            }
            _UnitOfWork.Product.Remove(ProductToBeDeleted);
            _UnitOfWork.Save();
            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}

