using System.Diagnostics;
using System.Security.Claims;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
    public class OrderController : Controller
	{
		public readonly IUnitOfWork _UnitOfWork;
		[BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
		{
			_UnitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Details(int orderId)
		{
			OrderVM = new()
			{
				OrderHeader = _UnitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetail = _UnitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
			};
			return View(OrderVM);
		}
		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult UpdateOrderDetail()
		{
			var getOrder = _UnitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
			getOrder.Name = OrderVM.OrderHeader.Name;
			getOrder.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
			getOrder.StreetAddress = OrderVM.OrderHeader.StreetAddress;
			getOrder.City = OrderVM.OrderHeader.City;
			getOrder.State = OrderVM.OrderHeader.State;
			getOrder.PostalCode = OrderVM.OrderHeader.PostalCode;
			if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
			{
				getOrder.Carrier = OrderVM.OrderHeader.Carrier;
			}
			if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
			{
				getOrder.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			}
			_UnitOfWork.OrderHeader.Update(getOrder);
			_UnitOfWork.Save();
			TempData["success"] = "Order details have been updated!";
			return RedirectToAction(nameof(Details), new { orderId = getOrder.Id });
		}
		[HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult StartProcessing()
		{
			_UnitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
			_UnitOfWork.Save();
            TempData["success"] = "Order details have been updated!";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var getOrder = _UnitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
			getOrder.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            getOrder.Carrier = OrderVM.OrderHeader.Carrier;
            getOrder.OrderStatus = SD.StatusShipped;
            getOrder.ShippingDate = DateTime.Now;
            if(getOrder.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				getOrder.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
			}
            _UnitOfWork.OrderHeader.Update(getOrder);
            _UnitOfWork.Save();
            TempData["success"] = "Order shipped successfully!";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var getOrder = _UnitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
			if(getOrder.PaymentStatus == SD.PaymentStatusApproved)
			{
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = getOrder.PaymentIntentId
				};
				var service = new RefundService();
				Refund refund = service.Create(options);
				_UnitOfWork.OrderHeader.UpdateStatus(getOrder.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
                _UnitOfWork.OrderHeader.UpdateStatus(getOrder.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _UnitOfWork.Save();
            TempData["success"] = "Order cancelled successfully!";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
		[ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
			OrderVM.OrderDetail = _UnitOfWork.OrderDetail.GetAll(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "Product");
            OrderVM.OrderHeader = _UnitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");

            var domain = "https://localhost:7271/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);
            _UnitOfWork.OrderHeader.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _UnitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _UnitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _UnitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _UnitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _UnitOfWork.Save();
                }
            }
            return View(orderHeaderId);
        }
        #region API CALLS

        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderList;
			if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
                orderList = _UnitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
			else
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderList = _UnitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
            }

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
