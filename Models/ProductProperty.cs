using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CafeMenuWebApp.Models
{
    public class ProductProperty
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PRODUCTPROPERYID { get; set; } 

        [ForeignKey("Product")]
        public int PRODUCTID { get; set; }  

        [ForeignKey("Property")]
        public int PROPERTYID { get; set; }
        public virtual Product? Product { get; set; } 
        public virtual Property? Property { get; set; }
        public int TenantId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
