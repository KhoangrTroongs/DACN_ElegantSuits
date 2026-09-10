using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElegantSuits.Domain.Entities;

public class FabricProduct
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Vải không được để trống")]
    [Display(Name = "Vải")]
    public int FabricId { get; set; }

    [Required(ErrorMessage = "Sản phẩm không được để trống")]
    [Display(Name = "Sản phẩm")]
    public int ProductId { get; set; }

    [Display(Name = "Còn hàng")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Ngày tạo")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("FabricId")]
    public virtual Fabric? Fabric { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
}
