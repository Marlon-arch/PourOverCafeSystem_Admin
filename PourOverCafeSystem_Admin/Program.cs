using Microsoft.EntityFrameworkCore;
using PourOverCafeSystem_Admin.Database;
using PourOverCafeSystem_Admin.Hubs;

namespace PourOverCafeSystem_Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            
            builder.Services.AddSignalR();
            
            builder.Services.AddDbContext<PourOverCoffeeDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("PourOverCoffeeDB"));
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowUserFrontend", policy =>
                {
                    policy.WithOrigins("https://localhost:7296")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // required for SignalR
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("AllowUserFrontend");

            app.UseAuthorization();

            app.MapHub<ReservationHub>("/reservationHub");

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
