using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NgoHuuDuc_2280600725.Models
{
    public class Fabric
    {
        public Fabric()
        {
            FabricProducts = new List<FabricProduct>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên vải không được để trống")]
        [StringLength(200, ErrorMessage = "Tên vải không được vượt quá 200 ký tự")]
        [Display(Name = "Tên vải")]
        public string Name { get; set; } = "";

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        [Display(Name = "Thành phần")]
        [StringLength(200, ErrorMessage = "Thành phần không được vượt quá 200 ký tự")]
        public string Composition { get; set; } = "";

        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; } = "";

        [Display(Name = "Giá vải")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } = 0;

        [Required(ErrorMessage = "Nhóm vải không được để trống")]
        [Display(Name = "Nhóm vải")]
        public int FabricGroupId { get; set; }

        [Display(Name = "Còn hàng")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key and navigation property
        [ForeignKey("FabricGroupId")]
        public virtual FabricGroup? FabricGroup { get; set; }

        // Navigation property for junction table
        public virtual ICollection<FabricProduct> FabricProducts { get; set; }
    }
}

