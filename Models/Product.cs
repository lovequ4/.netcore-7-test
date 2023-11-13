using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CartWebApi;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    [MaxLength(255)]
    public string Slug { get; set; }

    public string Description { get; set; }

    [Required]
    public int Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    public string Image { get; set; }

    public string? Thumbnail { get; set; }

    [Required]
    public DateTime DateAdded { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
}

