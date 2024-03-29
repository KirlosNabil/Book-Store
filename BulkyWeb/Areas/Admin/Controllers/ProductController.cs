﻿using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Construction;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            _UnitOfWork = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> ProductList = _UnitOfWork.Product.GetAll(includeProperties:"Category").ToList();
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
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.Trim('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if(productVM.Product.Id == 0)
                {
                    _UnitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Product created successfully!";
                }
                else
                {
					_UnitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Product updated successfully!";
                }
                _UnitOfWork.Save();
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

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> ProductList = _UnitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data =  ProductList});
        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            Product ProductToBeDeleted = _UnitOfWork.Product.Get(u => u.Id == Id);
            if(ProductToBeDeleted == null)
            {
                return Json(new {success = false, message = "Error while deleting"});
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, ProductToBeDeleted.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _UnitOfWork.Product.Remove(ProductToBeDeleted);
            _UnitOfWork.Save();
            return Json(new { success = true, message = "Deleted successfully!" });
        }

        #endregion
    }
}

