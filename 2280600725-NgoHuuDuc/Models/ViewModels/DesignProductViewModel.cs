using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Models.ViewModels
{
    public class DesignProductViewModel
    {
        public ProductDTO Product { get; set; }
        public IEnumerable<FabricGroupDTO> FabricGroups { get; set; }
    }
}

