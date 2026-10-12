using System.ComponentModel.DataAnnotations;

namespace GESTION_DES_ARTICLES.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Nom")]
        public string CategoryName { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}