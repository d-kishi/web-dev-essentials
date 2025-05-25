var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

/**
 * .NET9新機能
 * 静的アセット配信の最適化
 * https://learn.microsoft.com/ja-jp/aspnet/core/release-notes/aspnetcore-9.0?view=aspnetcore-9.0
 */
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Create}/{id?}"
).WithStaticAssets();

app.Run();
