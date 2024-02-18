using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
		private readonly ApplicationDbContext _db;
		public OrderHeaderRepository(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

		public void Update(OrderHeader orderHeader)
		{
			_db.Update(orderHeader);
		}
		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var getOrder = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if(getOrder != null)
			{
				getOrder.OrderStatus = orderStatus;
				if(!string.IsNullOrEmpty(paymentStatus))
				{
					getOrder.PaymentStatus = paymentStatus;
				}
			}
		}
		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var getOrder = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if(!string.IsNullOrEmpty(sessionId))
			{
				getOrder.SessionId = sessionId;
			}
			if(!string.IsNullOrEmpty(paymentIntentId))
			{
				getOrder.PaymentIntentId = paymentIntentId;
				getOrder.PaymentDate = DateTime.Now;
			}
		}
	}
}
