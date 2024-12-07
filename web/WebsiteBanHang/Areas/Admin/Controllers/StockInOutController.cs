using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteLaptopphukien.Models;
using System.Data.Entity;

namespace WebsiteLaptopphukien.Areas.Admin.Controllers
{
    public class StockInOutController : BaseController
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();

        // GET: Admin/StockInOut
        public ActionResult Index()
        {
            var stocks = db.StockInOuts
                .Include(x => x.User)
                .ToList();

            return View(stocks);
        }

        public JsonResult ChangeStatus(int id)
        {
            MStockInOut mProduct = db.StockInOuts.Find(id);
            mProduct.Status = (mProduct.Status == 1) ? 2 : 1;
            db.Entry(mProduct).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { Status = mProduct.Status });
        }

        [HttpGet]
        public ActionResult Create()
        {
            var users = db.Users.ToList();
            ViewBag.Users = users;

            var products = db.Products
                .Where(x => x.Status == 1)
                .ToList();
            ViewBag.Products = new SelectList(products, "ID", "Name");
           
            MStockInOut model = new MStockInOut();
            model.MStockInOutDetails.Add(new MStockInOutDetail());
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(MStockInOut model)
        {
            var users = db.Users.ToList();
            ViewBag.Users = users;

            var products = db.Products
                .Where(x => x.Status == 1)
                .ToList();
            ViewBag.Products = new SelectList(products, "ID", "Name");

            if (!ModelState.IsValid)
                return View(model);

            if (model.Type != "IN" && model.Type != "OUT")
            {
                ModelState.AddModelError(nameof(model.Type), "Vui lòng chọn loại phiếu !");
                return View(model);
            }    

            if (model.Status != 1 && model.Status != 2)
            {
                ModelState.AddModelError(nameof(model.Status), "Vui lòng chọn trạng thái phiếu !");
                return View(model);
            } 

            if (!model.CreateDate.HasValue)
                model.CreateDate = DateTime.Now;

            model.UserID = (int)Session["Admin_ID"];

            // Gộp mặt hàng

            var stockDetails = model.MStockInOutDetails
                .GroupBy(x => x.ProductID)
                .Select(x => new MStockInOutDetail()
                {
                    ID = 0,
                    StockInOutID = model.ID,
                    ProductID = x.Key,
                    Quantity = x.Sum(c => c.Quantity)
                }).ToList();

            model.MStockInOutDetails = stockDetails;

            db.StockInOuts.Add(model);

            for (int i = 0; i < model.MStockInOutDetails.Count; i++)
            {
                model.MStockInOutDetails[i].StockInOutID = model.ID;
                db.StockInOutDetails.Add(model.MStockInOutDetails[i]);
                var productId = model.MStockInOutDetails[i].ProductID;

                var product = db.Products.Where(x => x.ID == productId)
                    .FirstOrDefault();

                if (product != null)
                {
                    // Nếu là nhập hàng thì cộng dồn số lượng hàng
                    if (model.Type == "IN")
                    {
                        product.Quantity += model.MStockInOutDetails[i].Quantity;
                    }

                    if (model.Type == "OUT")
                    {
                        product.Quantity -= model.MStockInOutDetails[i].Quantity;
                    }

                    if (product.Quantity < 0)
                        product.Quantity = 0;
                    db.Products.Attach(product);
                    db.Entry(product).State = EntityState.Modified;

                }
            }

            db.SaveChanges();

            return RedirectToAction("Index", "StockInOut");
        }

        [HttpGet]
        public ActionResult Update(int Id)
        {
            var users = db.Users.ToList();
            ViewBag.Users = users;

            var products = db.Products
                .Where(x => x.Status == 1)
                .ToList();
            ViewBag.Products = new SelectList(products, "ID", "Name");

            var stock = db.StockInOuts
               .FirstOrDefault(x => x.ID == Id);

            if (stock == null)
                return RedirectToAction("Index", "StockInOut");


            return View(stock);
        }

        [HttpPost]
        public ActionResult Update(MStockInOut model)
        {
            var users = db.Users.ToList();
            ViewBag.Users = users;

            var products = db.Products
                .Where(x => x.Status == 1)
                .ToList();
            ViewBag.Products = new SelectList(products, "ID", "Name");

            if (!ModelState.IsValid)
                return View(model);

            if (model.Type != "IN" && model.Type != "OUT")
            {
                ModelState.AddModelError(nameof(model.Type), "Vui lòng chọn loại phiếu !");
                return View(model);
            }

            if (model.Status != 1 && model.Status != 2)
            {
                ModelState.AddModelError(nameof(model.Status), "Vui lòng chọn trạng thái phiếu !");
                return View(model);
            }

            // Gộp mặt hàng có chung mã hàng
            var stockDetails = model.MStockInOutDetails
               .GroupBy(x => x.ProductID)
               .Select(x => new MStockInOutDetail()
               {
                   ID = 0,
                   StockInOutID = model.ID,
                   ProductID = x.Key,
                   Quantity = x.Sum(c => c.Quantity)
               }).ToList();

            model.MStockInOutDetails = stockDetails;


            var stock = db.StockInOuts
                .FirstOrDefault(x => x.ID == model.ID);

            if (stock == null)
                return RedirectToAction("Index", "StockInOut");

            List<MStockInOutDetail> deleted = db.StockInOutDetails
                .Where(x => x.StockInOutID == stock.ID)
                .AsNoTracking()
                .ToList();

            // Xoá dữ liệu cũ
            for (int i = 0; i < deleted.Count; i++)
            {
                db.Entry(deleted[i]).State = EntityState.Deleted;

                var productId = deleted[i].ProductID;

                var product = db.Products.Where(x => x.ID == productId)
                    .FirstOrDefault();

                if (product != null)
                {
                    // Nếu là nhập hàng thì cộng dồn số lượng hàng
                    if (model.Type == "IN")
                    {
                        product.Quantity -= deleted[i].Quantity;
                    }

                    if (model.Type == "OUT")
                    {
                        product.Quantity += deleted[i].Quantity;
                    }

                    if (product.Quantity < 0)
                        product.Quantity = 0;
                }
            }

            // Cập Id cho dữ liệu mới
            for (int i = 0; i < model.MStockInOutDetails.Count; i++)
            {
                model.MStockInOutDetails[i].StockInOutID = stock.ID;

                var productId = model.MStockInOutDetails[i].ProductID;

                var product = db.Products.Where(x => x.ID == productId)
                    .FirstOrDefault();

                if (product != null)
                {
                    // Nếu là nhập hàng thì cộng dồn số lượng hàng
                    if (model.Type == "IN")
                    {
                        product.Quantity += model.MStockInOutDetails[i].Quantity;
                    }

                    if (model.Type == "OUT")
                    {
                        product.Quantity -= model.MStockInOutDetails[i].Quantity;
                    }

                    if (product.Quantity < 0)
                        product.Quantity = 0;
                }
            }

            stock.MStockInOutDetails = model.MStockInOutDetails;
            
            db.Entry(stock).CurrentValues.SetValues(model);

            db.SaveChanges();

            return RedirectToAction("Index", "StockInOut");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var stock = db.StockInOuts
                .FirstOrDefault(x => x.ID == id);

            if (stock == null)
                return RedirectToAction("Index", "StockInOut");

            List<MStockInOutDetail> deleteDetails = 
                db.StockInOutDetails
                .Where(x => x.StockInOutID == id).ToList();

            for (int i = 0; i < deleteDetails.Count; i++)
            {
                db.StockInOutDetails.Remove(deleteDetails[i]);

                var productID = deleteDetails[i].ProductID;

                var product = db.Products.Where(x => x.ID == productID)
                     .FirstOrDefault();

                if (product != null)
                {
                    // Nếu là nhập hàng thì cộng dồn số lượng hàng
                    if (stock.Type == "IN")
                    {
                        product.Quantity -= deleteDetails[i].Quantity;
                    }

                    if (stock.Type == "OUT")
                    {
                        product.Quantity += deleteDetails[i].Quantity;
                    }

                    if (product.Quantity < 0)
                        product.Quantity = 0;
                }
            } 

            db.StockInOuts.Remove(stock);

            db.SaveChanges();

            return RedirectToAction("Index", "StockInOut");
        }
    }
}