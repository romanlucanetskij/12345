using MvcXmlConfigApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ISettingsService, XmlSettingsService>();
builder.Services.AddSingleton<IStudentRepository, SqliteStudentRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
    await studentRepository.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Settings/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Settings}/{action=Index}/{id?}");

app.Run();
