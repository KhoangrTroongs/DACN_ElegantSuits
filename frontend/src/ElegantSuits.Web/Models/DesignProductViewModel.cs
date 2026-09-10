
namespace ElegantSuits.Web.Models
{
    public class DesignProductViewModel
    {
        public ProductDTO Product { get; set; }
        public IEnumerable<FabricGroupDTO> FabricGroups { get; set; }
    }
}

