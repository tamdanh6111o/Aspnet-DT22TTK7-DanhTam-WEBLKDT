using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteLaptopphukien.Models;
using System.Data.Entity;

namespace WebsiteLaptopphukien.Areas.Admin.Controllers
{
    public class ReportController : BaseController
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();
        // GET: Admin/Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TopTenBestSellerProducts()
        {
            List<ProductCategory> products = db.Products.Join(
                db.Orderdetails
                    .GroupBy(x => x.ProductID)
                    .Select(x => new { ProductId = x.Key, QuantitySum = x.Sum(a => a.Quantity) })
                    .OrderByDescending(x => x.QuantitySum)
                    .Select(x => new { x.ProductId, x.QuantitySum })
                    .Take(10), x => x.ID, y => y.ProductId, (x, y) => new { Product = x, QuantitySum = y.QuantitySum})
                .Join(db.Categorys, x => x.Product.CateID, y => y.ID, (x, y) => new ProductCategory()
                {
                    ProductId = x.Product.ID,
                    ProductImg = x.Product.Image,
                    ProductName = x.Product.Name,
                    ProductStatus = x.Product.Status,
                    ProductDiscount = x.Product.Discount,
                    QuantitySum = x.QuantitySum,
                    CategoryName = y.Name
                }).ToList();

            return View(products);
        }

    }
}