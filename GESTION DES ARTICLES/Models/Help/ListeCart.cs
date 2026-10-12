namespace GESTION_DES_ARTICLES.Models.Help
{
    public class ListeCart
    {
        public List<Item> Items { get; private set; } = new List<Item>();

        public static readonly ListeCart Instance = new ListeCart();

        protected ListeCart() { }

        public void AddItem(Product prod)
        {
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    a.quantite++;
                    return;
                }
            }
            Items.Add(new Item(prod) { quantite = 1 });
        }

        public void SetLessOneItem(Product prod)
        {
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    if (a.quantite <= 1) RemoveItem(a.Prod);
                    else a.quantite--;
                    return;
                }
            }
        }

        public void SetItemQuantity(Product prod, int quantity)
        {
            if (quantity == 0) { RemoveItem(prod); return; }
            foreach (Item a in Items)
            {
                if (a.Prod.ProductId == prod.ProductId)
                {
                    a.quantite = quantity;
                    return;
                }
            }
        }

        public void RemoveItem(Product prod)
        {
            Item? t = Items.FirstOrDefault(a => a.Prod.ProductId == prod.ProductId);
            if (t != null) Items.Remove(t);
        }

        public float GetSubTotal()
        {
            float subTotal = 0;
            foreach (Item i in Items) subTotal += i.TotalPrice;
            return subTotal;
        }
    }
}