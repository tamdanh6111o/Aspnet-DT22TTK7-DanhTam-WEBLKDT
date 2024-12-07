using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebsiteLaptopphukien.Models
{
    [Table("StockInOut")]
    public class MStockInOut
    {
        public MStockInOut()
        {
            CreateDate = DateTime.Now;
        }

        [Key]
        [Required]

        public int ID { get; set; }
        [Required(ErrorMessage = "Số phiếu nhập không được để trống")]
        public string Code { get; set; }
        public int UserID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CreateDate { get; set; }
        public string Note { get; set; }
        public string Type { get; set; } // IN - Nhập, OUT - Xuất

        public int Status { get; set; } = 1; // 1 -- Hoạt động, 2 -- Không hoạt động

        [ForeignKey(nameof(UserID))]
        public virtual MUser User { get; set; }
        public virtual List<MStockInOutDetail> MStockInOutDetails { get; set; } = new List<MStockInOutDetail>();
    }
}