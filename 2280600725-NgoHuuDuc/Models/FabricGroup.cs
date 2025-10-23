using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NgoHuuDuc_2280600725.Models
{
    public class FabricGroup
    {
        public FabricGroup()
        {
            Fabrics = new List<Fabric>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nhóm vải không được để trống")]
        [StringLength(100, ErrorMessage = "Tên nhóm vải không được vượt quá 100 ký tự")]
        [Display(Name = "Tên nhóm vải")]
        public string Name { get; set; } = "";

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Fabric> Fabrics { get; set; }
    }
}

