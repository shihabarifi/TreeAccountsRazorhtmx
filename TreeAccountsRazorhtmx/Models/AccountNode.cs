namespace TreeAccountsRazorhtmx.Models
{
    public class AccountNode
    {
        public Account Account { get; set; } = null!;
        public List<AccountNode> Children { get; set; } = new List<AccountNode>();
    }
}
