using System.ComponentModel.DataAnnotations;

namespace ElegantSuits.Domain.Enums;

public enum Gender
{
    [Display(Name = "Nam")]
    Male = 0,
    [Display(Name = "Nữ")]
    Female = 1,
    [Display(Name = "Khác")]
    Other = 2
}
