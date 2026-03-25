using BookGuide.API.Data;
using BookGuide.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookGuideDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IEmailSender, EmailSender>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookGuideDbContext>();

    try
    {
        Console.WriteLine("DB: " + db.Database.GetDbConnection().DataSource);
        Console.WriteLine("DatabaseName: " + db.Database.GetDbConnection().Database);

        await db.Database.EnsureCreatedAsync();

        Console.WriteLine("Database ensured successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB ERROR FULL: " + ex.ToString());
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapGet("/", () => "BookGuide API is running");
app.MapGet("/ping", () => "pong");

app.MapControllers();

app.Run();