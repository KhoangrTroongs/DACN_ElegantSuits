namespace NgoHuuDuc_2280600725.DTOs
{
    public class FabricDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Composition { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal Price { get; set; }
        public int FabricGroupId { get; set; }
        public string? FabricGroupName { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FabricGroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<FabricDTO>? Fabrics { get; set; }
    }

    public class CreateFabricDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Composition { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal Price { get; set; }
        public int FabricGroupId { get; set; }
    }

    public class UpdateFabricDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Composition { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal Price { get; set; }
        public int FabricGroupId { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateFabricGroupDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int DisplayOrder { get; set; }
    }

    public class UpdateFabricGroupDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int DisplayOrder { get; set; }
    }
}

