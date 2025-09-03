using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TreeAccountsRazorhtmx.Models;
using System.Net.Http.Json;


namespace TreeAccountsRazorhtmx.Pages
{
    public class AccountsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // ‰” Œœ„Â« ·⁄—÷ «·Ã–— ›ﬁÿ
        public List<Account> RootAccounts { get; set; } = new List<Account>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("UrlApi");
            try
            {
                var response = await client.GetAsync("accounts");
                if (response.IsSuccessStatusCode)
                {
                    var accounts = await response.Content.ReadFromJsonAsync<List<Account>>();
                    if (accounts != null)
                    {
                        // ≈⁄œ«œ RootAccounts
                        RootAccounts = accounts
                            .Where(a => a.FatherNumber == "0" && a.AccountNumber != "9")
                            .ToList();

                        // ÷⁄ ⁄·«„… ≈–« ⁄‰œÂ √»‰«¡
                        foreach (var acc in accounts)
                        {
                            acc.HasChildren = accounts.Any(c => c.FatherNumber == acc.AccountNumber && c.AccountNumber != "9");
                        }
                    }
                }
            }
            catch
            {
                RootAccounts = new List<Account>();
            }
        }

        public async Task<PartialViewResult> OnGetChildrenAsync(string fatherNumber)
        {
            var client = _httpClientFactory.CreateClient("UrlApi");
            List<Account> children = new();

            try
            {
                var response = await client.GetAsync("accounts");
                if (response.IsSuccessStatusCode)
                {
                    var accounts = await response.Content.ReadFromJsonAsync<List<Account>>();
                    if (accounts != null)
                    {
                        children = accounts
                            .Where(a => a.FatherNumber == fatherNumber && a.AccountNumber != "9")
                            .ToList();

                        // ÷⁄ ⁄·«„… ≈–« ⁄‰œÂ √»‰«¡
                        foreach (var acc in children)
                        {
                            acc.HasChildren = accounts.Any(c => c.FatherNumber == acc.AccountNumber && c.AccountNumber != "9");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching accounts: {ex.Message}");
            }

            return Partial("_AccountChildrenPartial", children);
        }


    }



}
