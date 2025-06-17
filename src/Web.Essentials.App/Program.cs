using Microsoft.EntityFrameworkCore;
using Web.Essentials.Infrastructure.Data;
using Web.Essentials.Domain.Repositories;
using Web.Essentials.Infrastructure.Repositories;
using Web.Essentials.App.Services;
using Web.Essentials.App.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Entity Framework Core with InMemory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("WebEssentialsDb"));

// Repository registrations
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();

// Application service registrations
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
// インターフェース実装完了により有効化
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
