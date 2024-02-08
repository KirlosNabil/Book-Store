using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product product)
        {
            Product getProduct = _db.Products.FirstOrDefault(u => u.Id == product.Id);
            if(getProduct != null)
            {
                getProduct.Title = product.Title;
				getProduct.ISBN = product.ISBN;
				getProduct.Description = product.Description;
				getProduct.Author = product.Author;
				getProduct.ListPrice = product.ListPrice;
				getProduct.Price100 = product.Price100;
                getProduct.Price = product.Price;
				getProduct.Price50 = product.Price50;
				getProduct.CategoryId = product.CategoryId;
                if(getProduct.ImageUrl != null)
                {
                    getProduct.ImageUrl = product.ImageUrl;
                }
			}
        }
    }
}
