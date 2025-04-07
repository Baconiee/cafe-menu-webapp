using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CafeMenuWebApp.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int CreatorUserId { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductProperty> ProductProperties { get; set; } = new List<ProductProperty>(); 
        public virtual ICollection<Property> Properties { get; set; } = new List<Property>();

        public int TenantId { get; set; }

        [NotMapped]
        public int[]? SelectedPropertyIds { get; set; }

    }
}
