using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CafeMenuWebApp.Models
{
    public class Property
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PROPERTYID { get; set; }

        public string KEY { get; set; }

        public string VALUE { get; set; }
        public int TenantId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
