namespace TreeAccountsRazorhtmx.Models
{
    public class AccountNodeViewModel
    {
        public Account Account { get; set; } = default!;
        public List<Account> AllAccounts { get; set; } = new();
    }
}
