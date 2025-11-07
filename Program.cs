using BookVectorMVC.Data;
using BookVectorMVC.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // 支援 Razor 頁面即時編譯

// 配置 Entity Framework
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 註冊服務
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IEnhancedBookService, EnhancedBookService>();
builder.Services.AddSingleton<IApiService, ApiService>();

// 配置 HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 配置路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Book}/{action=Index}/{id?}");

// 確保資料庫已建立
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    context.Database.EnsureCreated();
}

app.Run();