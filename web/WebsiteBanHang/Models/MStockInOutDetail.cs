using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebsiteLaptopphukien.Models
{
    [Table("StockInOutDetail")]
    public class MStockInOutDetail
    {
        [Key]
        [Required]
        public int ID { get; set; }
        public int StockInOutID { get; set; }
        public int ProductID { get; set; }
        [Required]
        [Range(1, 100000, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
        [ForeignKey(nameof(StockInOutID))]
        public virtual MStockInOut StockInOut { get; set; }
        [ForeignKey(nameof(ProductID))]
        public virtual MProduct Product { get; set; }
    }
}