using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net.Http.Json;
using TreeAccountsRazorhtmx.Models;

namespace TreeAccountsRazorhtmx.Pages
{
    public class AccountssModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<Account> RootAccounts { get; set; } = new List<Account>();
        public List<Account> FatherAccountList { get; set; } = new List<Account>();
        public Account AccountEidter { get; set; } = new Account();

        // يمكن تخزين كل الحسابات هنا لتحسين الأداء
        private static List<Account>? allAccountsCache;

        public AccountssModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // هذا المعالج لجلب العقد الجذرية عند تحميل الصفحة
        public async Task OnGetAsync()
        {
            await FetchAndSetRootAccounts();
        }

       

        // دالة مساعدة لجلب البيانات من الـ API وتخزينها
        private async Task FetchAndSetRootAccounts()
        {
          
                var client = _httpClientFactory.CreateClient("UrlApi");
                try
                {
                    var response = await client.GetAsync("accounts");
                    if (response.IsSuccessStatusCode)
                    {
                        allAccountsCache = await response.Content.ReadFromJsonAsync<List<Account>>() ?? new List<Account>();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching accounts: {ex.Message}");
                    allAccountsCache = new List<Account>();
                }
            

            // تحديد العقد الجذرية وتعيين خاصية HasChildren
            FatherAccountList = allAccountsCache.Where(a => a.AccountType == "رئيسي").ToList();
            RootAccounts = allAccountsCache
                .Where(a => a.FatherNumber == "0" && a.AccountNumber != "9")
                .ToList();

            foreach (var account in RootAccounts)
            {
                account.HasChildren = allAccountsCache.Any(a => a.FatherNumber == account.AccountNumber);
            }
        }

        #region save & Update Account
        public async Task<PartialViewResult> OnPostSaveAccount()
        {
            await Task.Delay(3000); // محاكاة عملية حفظ
            // الحصول على البيانات من النموذج
            var accountName = Request.Form["AccountName"];
            var fatherNumber = Request.Form["FatherNumber"];
            var accountNumber = Request.Form["AccountNumber"];
            var accountNameEng = Request.Form["AccountNameEng"];
            var finalAccount = Request.Form["FinalAccount"];
            var accountType = Request.Form["AccountType"];

            // 1. منطق التحقق من صحة البيانات (Validation)
            // - تأكد من أن البيانات غير فارغة
            // - تحقق من وجود رقم الحساب في قاعدة البيانات
            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountNumber))
            {
                // يمكنك إرجاع رسالة خطأ
                ViewData["Message"] = "الرجاء تعبئة الحقول المطلوبة.";
                return Partial("_FormPartial", this); // أرجع النموذج مع رسالة الخطأ
            }

            // 2. إنشاء كائن الحساب الجديد
            var newAccount = new Account
            {
                AccountName = accountName,
                FatherNumber = fatherNumber,
                AccountNumber = accountNumber,
                AccountNameEng = accountNameEng,
                AccountReference = finalAccount,
                AccountType = accountType,
             

            };

            try
            {
                // 3. حفظ البيانات في قاعدة البيانات
                var client = _httpClientFactory.CreateClient("UrlApi");
                AlertViewModel alert ;
                    var response = await client.PostAsJsonAsync("accounts", newAccount);
                    if (response.IsSuccessStatusCode)
                    {
                    // Read the content of the response and deserialize it into an Account object.
                    var createdAccount = await response.Content.ReadFromJsonAsync<Account>();

                    // Now you can access the new ID from the createdAccount object.
                    newAccount.ID= createdAccount.ID;

                   
                    // إضافة الحساب الجديد إلى الكاش مباشرة لتحديث الشجرة
                    allAccountsCache.Add(newAccount);

               

                    // 4. إعداد رسالة نجاح
                    var alert1 = new AlertViewModel
                {
                    Title = "نجاح",
                    Message = "تمت إضافة الحساب بنجاح",
                    Icon = "success",
                    IsToast = true,
                    Position = "top-end",
                    Timer = 4000
                };

                 alert = new AlertViewModel
                {
                    Title = "نجاح العملية",
                    Message = $"تمت إضافة الحساب : {accountName}",
                    Icon = "success",
                    ConfirmButtonText = "موافق",
                    Timer = 3000
                };
                }
                else
                    {
                     alert = new AlertViewModel
                    {
                        Title = "فشل عملية الاضافة",
                        Message = $"خطأ في اضافة الحساب : {response.RequestMessage}",
                        Icon = "erorr",
                        ConfirmButtonText = "موافق",
                        Timer = 3000
                    };

                }

                    // 6. ارجع الجزء المحدث من الصفحة (النموذج الفارغ أو رسالة النجاح)
                    return Partial("_SaveResultPartial", alert);
                
               
            }
            catch (Exception ex)
            {
                // 7. التعامل مع الأخطاء وإرجاع رسالة خطأ
                ViewData["Message"] = "حدث خطأ أثناء الحفظ: " + ex.Message;
                return Partial("_FormPartial", this);
            }
        }
        public async Task<PartialViewResult> OnPostUpdateAccount()
        {
            // Retrieve the account number from the form.
            var accountNumber = Request.Form["AccountNumber"].ToString();

            // Find the existing account in the database.
            var accountToUpdate = allAccountsCache.FirstOrDefault(a => a.AccountNumber == accountNumber);

            // Check if the account exists.
            if (accountToUpdate == null)
            {
                // Handle the case where the account is not found.
                var notFoundAlert = new AlertViewModel
                {
                    Title = "فشل العملية",
                    Message = "حدث خطأ: الحساب غير موجود.",
                    Icon = "error",
                    ConfirmButtonText = "موافق",
                    Timer = 3000
                };
                return Partial("_SaveResultPartial", notFoundAlert);
            }

            // Update the account's properties with the new form values.
            // Use try-catch for error handling.
            try
            {
                accountToUpdate.AccountName = Request.Form["AccountName"].ToString();
                accountToUpdate.AccountNameEng = Request.Form["AccountNameEng"].ToString();
                accountToUpdate.FatherNumber = Request.Form["FatherNumber"].ToString();
                accountToUpdate.AccountReference = Request.Form["FinalAccount"].ToString();
                accountToUpdate.AccountType = Request.Form["AccountType"].ToString();

                // Save the changes to the database.
                var client = _httpClientFactory.CreateClient("UrlApi");
                AlertViewModel alert;
                var response = await client.PutAsJsonAsync($"accounts/{accountToUpdate.ID}", accountToUpdate);

                if (response.IsSuccessStatusCode)
                {
                    alert = new AlertViewModel
                    {
                        Title = "نجاح العملية",
                        Message = $"تم تعديل الحساب: {accountToUpdate.AccountName}",
                        Icon = "success",
                        ConfirmButtonText = "موافق",
                        Timer = 3000
                    };
                }
                else
                {
                    // Handle API errors and read the specific error message from the response.
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    alert = new AlertViewModel
                    {
                        Title = "فشل العملية",
                        Message = $"حدث خطأ في تحديث الحساب: {errorMessage}",
                        Icon = "error",
                        ConfirmButtonText = "موافق",
                        Timer = 3000
                    };
                }

                // To refresh the tree view on the page, you can send an HTMX event
                // with a custom header.
                Response.Headers.Add("HX-Trigger", "accountUpdated");

                // Return a partial view with the message.
                return Partial("_SaveResultPartial", alert);
            }
            catch (Exception ex)
            {
                // Handle network or serialization errors.
                var errorAlert = new AlertViewModel
                {
                    Title = "خطأ غير متوقع",
                    Message = $"حدث خطأ أثناء الاتصال بالخادم: {ex.Message}",
                    Icon = "error",
                    ConfirmButtonText = "موافق",
                    Timer = 3000
                };
                return Partial("_SaveResultPartial", errorAlert);
            }
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
                return BadRequest();

            // هنا تكتب منطق الحذف من قاعدة البيانات أو API
            var account = allAccountsCache?.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account != null)
            {
                allAccountsCache?.Remove(account);
            }

            // نرجع Partial فاضي عشان htmx يحذف العنصر
            return Content(string.Empty);
        }

        #endregion

        #region Helpers partialView
        // هذا المعالج الجديد لجلب العقد الفرعية بناءً على رقم الأب
        public async Task<PartialViewResult> OnGetChildrenAsync(string fatherNumber)
        {
            if (allAccountsCache == null)
            {
                await FetchAndSetRootAccounts(); // التأكد من تحميل البيانات إذا لم تكن موجودة
            }

            // جلب الأبناء من الذاكرة المؤقتة (cache) بدلاً من إعادة الاتصال بالـ API
            var children = allAccountsCache
                ?.Where(a => a.FatherNumber == fatherNumber && a.FatherNumber != "9")
                .ToList() ?? new List<Account>();

            // تعيين خاصية HasChildren لكل حساب فرعي قبل إرساله إلى الصفحة الجزئية
            foreach (var account in children)
            {
                account.HasChildren = allAccountsCache!.Any(a => a.FatherNumber == account.AccountNumber);
            }

            return Partial("_AccountChildrenPartial", children);
        }

        public async Task<PartialViewResult> OnGetSearch(string query)
        {
            if (allAccountsCache == null)
            {
                await FetchAndSetRootAccounts();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                // رجّع الجذور لو مافي بحث
                var roots = allAccountsCache.Where(a => a.FatherNumber == "0" && a.AccountNumber != "9").ToList();
                return Partial("_AccountChildrenPartial", roots);
            }

            // البحث بالاسم أو رقم الحساب
            var matches = allAccountsCache
                .Where(a => a.AccountName.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || a.AccountNumber.Contains(query) && a.FatherNumber != "9")
                .ToList();

            // تجهيز خاصية HasChildren
            foreach (var account in matches)
            {
                account.HasChildren = allAccountsCache.Any(a => a.FatherNumber == account.AccountNumber);
            }

            return Partial("_AccountChildrenPartial", matches);
        }

        public async Task<PartialViewResult> OnGetGetNewAccountNumber(string fatherNumber, string accountType)
        {
            // هنا تقوم بمنطقك لحساب الرقم الجديد
            // يمكنك جلب آخر رقم حساب فرعي من قاعدة البيانات
            // أو ببساطة إضافة +1 إلى رقم الحساب الأب
            var newAccountNumber = $"{fatherNumber}1"; // مثال: إضافة +1 كبداية

            // يمكنك تحسين هذا المنطق ليكون أكثر دقة
            // مثلاً: جلب آخر رقم حساب فرعي لهذا الأب من DB ثم +1
            // ...
            var lastChild = allAccountsCache.Where(a => a.FatherNumber == fatherNumber && a.AccountType== accountType).OrderByDescending(a => a.AccountNumber).FirstOrDefault();
            if (lastChild != null)
            {
                newAccountNumber =  (long.Parse(lastChild.AccountNumber) + 1).ToString();
            }
            else
            {
                
                newAccountNumber = accountType!="رئيسي" ?$"{fatherNumber}001": $"{fatherNumber}1";
            }

            // هنا نقوم بإنشاء جزء HTML الذي سيتم إرجاعه
            return Partial("_AccountNumberInputPartial", newAccountNumber);
           
        }

        public PartialViewResult OnGetFatherAccountSelectAsync(string fatherNumber)
        {
            var model = new FatherAccountSelectViewModel
            {
                FatherNumber = fatherNumber,
                Accounts = allAccountsCache.ToList()
            };

            return Partial("_FatherAccountSelectPartial", model);
        }

        public async Task<PartialViewResult> OnGetEditAccount(string accountNumber)
        {
            // Find the account in the database using the provided account number.
            var accountToEdit =allAccountsCache.FirstOrDefault(a => a.AccountNumber == accountNumber );

            // Check if the account exists.
            if (accountToEdit == null)
            {
                // Handle the case where the account is not found.
                // You can return a message or a different partial view.
              var  alert = new AlertViewModel
                {
                    Title = "فشل عملية الاضافة",
                    Message = $"خطأ Account not found. : {accountNumber}",
                    Icon = "erorr",
                    ConfirmButtonText = "موافق",
                    Timer = 3000
                };

                var fatherAccountList = allAccountsCache.Where(a => a.AccountType == "رئيسي").ToList();

                // 6. ارجع الجزء المحدث من الصفحة (النموذج الفارغ أو رسالة النجاح)
                return Partial("_CreateAccountPartial", fatherAccountList);
            }

            // Pass both the account to edit and the list of father accounts to the partial view.
            return Partial("_EditAccountPartial", new Tuple<Account, List<Account>>(accountToEdit, allAccountsCache.ToList()));

        }

        public async Task<PartialViewResult> OnGetCreateAccountPartial()
        {
            // جلب قائمة الحسابات الأب لإرسالها إلى _CreateAccountPartial
            var fatherAccountList =allAccountsCache.Where(a => a.AccountType == "رئيسي").ToList();

            // تمرير القائمة مباشرة إلى Partial View
            return Partial("_CreateAccountPartial", fatherAccountList);
        }



        #endregion
    }
}
