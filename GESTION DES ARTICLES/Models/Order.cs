using Microsoft.AspNetCore.Identity;

namespace GESTION_DES_ARTICLES.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public float TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        // Lien avec l'utilisateur Identity
        public string UserId { get; set; } = "";
        public IdentityUser? User { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}