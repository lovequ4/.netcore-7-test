using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using CartWebApi;

public class Cart
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; }  //IdentityUser id是string，改用string
    [ForeignKey("UserId")]
    public IdentityUser user { get; set; }
    [Required]
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product product { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public int Price { get; set; }
}

