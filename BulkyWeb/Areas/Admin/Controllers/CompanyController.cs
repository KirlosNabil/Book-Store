using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.ViewModels;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]

	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _UnitOfWork;
		public CompanyController(IUnitOfWork db)
		{
			_UnitOfWork = db;
		}
		public IActionResult Index()
		{
			List<Company> CompanyList = _UnitOfWork.Company.GetAll().ToList();
			return View(CompanyList);
		}
		public IActionResult Upsert(int? Id)
		{
			Company company = new Company();
			if (Id != null && Id != 0)
			{
				 company = _UnitOfWork.Company.Get(u => u.Id == Id);
			}
			return View(company);
		}
		[HttpPost]
		public IActionResult Upsert(Company Company)
		{
			if (ModelState.IsValid)
			{
				if (Company.Id == 0)
				{
					_UnitOfWork.Company.Add(Company);
					TempData["success"] = "Company created successfully!";
				}
				else
				{
					_UnitOfWork.Company.Update(Company);
					TempData["success"] = "Company updated successfully!";
				}
				_UnitOfWork.Save();
				return RedirectToAction("Index");
			}
			else
			{
				return View(Company);
			}
		}

		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			List<Company> CompanyList = _UnitOfWork.Company.GetAll().ToList();
			return Json(new { data = CompanyList });
		}

		[HttpDelete]
		public IActionResult Delete(int? Id)
		{
			Company CompanyToBeDeleted = _UnitOfWork.Company.Get(u => u.Id == Id);
			if (CompanyToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
			_UnitOfWork.Company.Remove(CompanyToBeDeleted);
			_UnitOfWork.Save();
			return Json(new { success = true, message = "Deleted successfully!" });
		}

		#endregion
	}
}
