using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteLaptopphukien.Models;
using System.Data.Entity;

namespace WebsiteLaptopphukien.Areas.Admin.Controllers
{
    public class RatingController : Controller
    {
        private WebsiteBanDoGiaDungDbContext db = new WebsiteBanDoGiaDungDbContext();

        public ActionResult Index()
        {
            var items = db.Orderdetails
                .Include(c => c.Product)
                .Where(x => x.Rating > 0)
                .OrderByDescending(x => x.RatingDate)
                .ToList();

            return View(items);
        }

        public ActionResult Delete(int Id)
        {
            var item = db.Orderdetails
                .Where(x => x.ID == Id)
                .FirstOrDefault();

            if (item == null)
                return RedirectToAction("Index", "Rating");

            item.Rating = null;
            item.RatingNote = "";
            item.RatingDate = null;

            db.Entry(item).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return RedirectToAction("Index", "Rating");
                
        }
    }
}