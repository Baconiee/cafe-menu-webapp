using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CafeMenuWebApp.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int USERID { get; set; }

        [Required]
        public string NAME { get; set; }

        [Required]
        public string SURNAME { get; set; }

        [Required]
        public string USERNAME { get; set; }

        [Required]
        public byte[] HASHPASSWORD { get; set; }

        [Required]
        public byte[] SALTPASSWORD { get; set; }
        public int TenantId { get; set; }

    }
}
