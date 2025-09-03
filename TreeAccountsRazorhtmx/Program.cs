var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("UrlApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5245/"); // €Ì¯— «·—«»ÿ Õ”» «·‹ Minimal API ⁄‰œﬂ
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
