using GESTION_DES_ARTICLES.Models;

namespace GESTION_DES_ARTICLES.ViewModels
{
    public class ProduitPaginationViewModel
    {
        public List<Product> Products { get; set; } = new();
        public int PageActuelle { get; set; }
        public int TotalPages { get; set; }
    }
}