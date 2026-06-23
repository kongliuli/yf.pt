using Yf.Pt.Components;
using Yf.Pt.CloudProviders;
using Yf.Pt.Data;
using Yf.Pt.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// 云资源提供者（当前 Mock，后续切换为 Aliyun 实现）
builder.Services.AddSingleton<ICloudResourceProvider, MockCloudResourceProvider>();
builder.Services.AddSingleton<IAlertProvider, MockAlertProvider>();
builder.Services.AddSingleton<ICostProvider, MockCostProvider>();

// 业务服务
builder.Services.AddScoped<AppProjectService>();
builder.Services.AddScoped<DomainBindingService>();
builder.Services.AddScoped<LocalResourceService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<KnowledgeService>();
builder.Services.AddScoped<ExpiryCheckService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<CostService>();
builder.Services.AddScoped<TopologyService>();

var app = builder.Build();

// 初始化数据库 + 种子数据
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
