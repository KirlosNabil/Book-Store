using System.Diagnostics;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
	{
		public readonly IUnitOfWork _UnitOfWork;
		public OrderController(IUnitOfWork unitOfWork)
		{
			_UnitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}
		#region API CALLS

		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderList = _UnitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

			switch (status)
			{
				case "pending":
					orderList = orderList.Where(u => u.OrderStatus == SD.StatusPending);
					break;
				case "inprocess":
					orderList = orderList.Where(u => u.OrderStatus == SD.StatusInProcess);
					break;
				case "completed":
					orderList = orderList.Where(u => u.OrderStatus == SD.StatusShipped);
					break;
				case "approved":
					orderList = orderList.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
				default:
					break;
			}

			return Json(new { data = orderList });
		}

		#endregion
	}
}
