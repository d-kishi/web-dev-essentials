var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

/**
 * .NET9�V�@�\
 * �ÓI�A�Z�b�g�z�M�̍œK��
 * https://learn.microsoft.com/ja-jp/aspnet/core/release-notes/aspnetcore-9.0?view=aspnetcore-9.0
 */
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Create}/{id?}"
).WithStaticAssets();

app.Run();
