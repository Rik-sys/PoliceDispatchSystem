
using BLL;
using DAL;
using DBEntities.Models;
using IBL;
using IDAL;
using Microsoft.EntityFrameworkCore;

namespace PoliceDispatchSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost8080",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:8080")
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IGraphService, GraphService>();
            builder.Services.AddScoped<IKCenterService, KCenterService>();
            builder.Services.AddScoped<IEventDAL, EventDAL>();
            builder.Services.AddScoped<IEventService, EventService>();

            builder.Services.AddDbContext<PoliceDispatchSystemContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowLocalhost8080");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
