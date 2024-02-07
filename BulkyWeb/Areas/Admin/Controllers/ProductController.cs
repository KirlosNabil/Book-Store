using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public IActionResult Upsert(int? Id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _UnitOfWork.Category.GetAll().Select(
				u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString(),
				}
			    ),
                Product = new Product()
            };
            if(Id != null && Id != 0)
            {
                productVM.Product = _UnitOfWork.Product.Get(u=> u.Id==Id);
			}
			return View(productVM);
		}
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.Product.Add(productVM.Product);
                _UnitOfWork.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _UnitOfWork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
				return View(productVM);
			}
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

