namespace TreeAccountsRazorhtmx.Models
{
    public static class AccountExtensions
    {
        public static List<AccountNode> BuildTree(this List<Account> accounts)
        {
            var lookup = accounts.ToDictionary(a => a.ID, a => new AccountNode { Account = a });
            var roots = new List<AccountNode>();

            foreach (var node in lookup.Values)
            {
                if (node.Account.FatherID.HasValue && lookup.ContainsKey(node.Account.FatherID.Value))
                {
                    lookup[node.Account.FatherID.Value].Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            return roots;
        }
    }
}
