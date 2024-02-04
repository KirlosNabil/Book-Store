using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkyRazorWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(25)]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 50, ErrorMessage = "Invalid display order, range is 1-100")]
        public int DisplayOrder { get; set; }

    }
}
