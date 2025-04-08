using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CafeMenuWebApp.Models
{
    public class EditCategoryViewModel
    {
        public int CATEGORYID { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        public string CATEGORYNAME { get; set; }

        public int? PARENTCATEGORYID { get; set; }

        public string? CurrentImagePath { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
