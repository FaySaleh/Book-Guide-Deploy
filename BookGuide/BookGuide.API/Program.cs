using BookGuide.API.Data;
using BookGuide.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AchievementsService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<NotificationsService>();

builder.Services.AddDbContext<BookGuideDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services.AddHostedService<ReminderHostedService>();

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://bookguide-ui.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// builder.Services.AddHostedService<ReminderHostedService>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookGuideDbContext>();

    try
    {
        Console.WriteLine("DB: " + db.Database.GetDbConnection().DataSource);
        Console.WriteLine("DatabaseName: " + db.Database.GetDbConnection().Database);

        var created = await db.Database.EnsureCreatedAsync();
        Console.WriteLine("EnsureCreated result: " + created);

        await AchievementsSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Startup DB creation failed: " + ex.ToString());
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();