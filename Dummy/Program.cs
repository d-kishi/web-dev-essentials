var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// HTTP リクエスト パイプラインを構成します。
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // デフォルトの HSTS 値は 30 日です。運用環境のシナリオでは、これを変更することを検討してください。詳細は https://aka.ms/aspnetcore-hsts を参照してください。
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
