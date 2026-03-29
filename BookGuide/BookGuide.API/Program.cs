using BookGuide.API.Data;
using BookGuide.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<BookGuideDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddHttpClient();

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<NotificationsService>();
builder.Services.AddScoped<AchievementsService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<ReminderHostedService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://bookguide-ui.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookGuideDbContext>();

    try
    {
        await db.Database.EnsureCreatedAsync();
        Console.WriteLine("Database ensured successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Database error: " + ex.Message);
        Console.WriteLine(ex.ToString());
    }
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookGuideDbContext>();

    try
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Achievements.AnyAsync())
        {
            db.Achievements.AddRange(
                new BookGuide.API.Models.Achievement
                {
                    Code = "FIRST_BOOK",
                    Title = "First Book",
                    Description = "Finish your first book.",
                    Icon = "📘",
                    TargetValue = 1
                },
                new BookGuide.API.Models.Achievement
                {
                    Code = "FIVE_BOOKS",
                    Title = "Five Books",
                    Description = "Finish 5 books.",
                    Icon = "📚",
                    TargetValue = 5
                },
                new BookGuide.API.Models.Achievement
                {
                    Code = "TEN_BOOKS",
                    Title = "Ten Books",
                    Description = "Finish 10 books.",
                    Icon = "🏆",
                    TargetValue = 10
                },
                new BookGuide.API.Models.Achievement
                {
                    Code = "PAGES_1000",
                    Title = "Page Master",
                    Description = "Read 1000 pages.",
                    Icon = "📖",
                    TargetValue = 1000
                },
                new BookGuide.API.Models.Achievement
                {
                    Code = "STREAK_7",
                    Title = "7-Day Streak",
                    Description = "Read for 7 days in a row.",
                    Icon = "🔥",
                    TargetValue = 7
                }
            );

            await db.SaveChangesAsync();
            Console.WriteLine("Achievements seeded successfully.");
        }

        Console.WriteLine("Database ensured successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Database error: " + ex.Message);
        Console.WriteLine(ex.ToString());
    }
}

app.Run();