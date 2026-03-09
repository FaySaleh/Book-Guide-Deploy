using BookGuide.API.Data;
using BookGuide.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AchievementsService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

//builder.Services.AddScoped<IEmailSender();
//builder.Services.AddScoped <SmtpEmailSender > ();


builder.Services.AddScoped<NotificationsService>();

builder.Services.AddDbContext<BookGuideDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
            .WithOrigins(
                "https://bookguide-ui.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<BookGuideDbContext>();

//    Console.WriteLine("DB: " + db.Database.GetDbConnection().DataSource);
//    Console.WriteLine("DatabaseName: " + db.Database.GetDbConnection().Database);

//    await db.Database.MigrateAsync();
//    await AchievementsSeeder.SeedAsync(db);
//}


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
