using System.ComponentModel.DataAnnotations;

namespace GESTION_DES_ARTICLES.ViewModels
{
    public class OrderViewModel
    {
        [Required]
        public string Address { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public float TotalAmount { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new();
    }
}