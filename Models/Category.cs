using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CafeMenuWebApp.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CATEGORYID { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        public string CATEGORYNAME { get; set; }

        public int? PARENTCATEGORYID { get; set; }

        public bool ISDELETED { get; set; }

        public DateTime CREATEDDATE { get; set; } = DateTime.Now;

        public int CreatorUserId { get; set; }

        public string? ImagePath { get; set; }

        public int TenantId { get; set; }

    }
}
