var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// HTTP ���N�G�X�g �p�C�v���C�����\�����܂��B
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // �f�t�H���g�� HSTS �l�� 30 ���ł��B�^�p���̃V�i���I�ł́A�����ύX���邱�Ƃ��������Ă��������B�ڍׂ� https://aka.ms/aspnetcore-hsts ���Q�Ƃ��Ă��������B
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
