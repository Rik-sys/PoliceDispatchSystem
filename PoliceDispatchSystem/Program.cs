
using BLL;
using DAL;
using DAL.DAL;
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

            DTO.ConfigurationLoader.LoadOnewaySettings(builder.Configuration);

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
            builder.Services.AddSingleton<IGraphManagerService, GraphManagerService>();
            builder.Services.AddScoped<IGraphService, GraphService>();

            builder.Services.AddScoped<IGraphService, GraphService>();
            builder.Services.AddScoped<IKCenterService, KCenterService>();
            builder.Services.AddScoped<IEventDAL, EventDAL>();
            builder.Services.AddScoped<IEventService, EventService>();

            builder.Services.AddScoped<IOfficerAssignmentDAL, OfficerAssignmentDAL>();
            builder.Services.AddScoped<IOfficerAssignmentService, OfficerAssignmentService>();
            builder.Services.AddScoped<IPoliceOfficerDAL, PoliceOfficerDAL>();
            builder.Services.AddScoped<IUserDAL, UserDAL>();

            builder.Services.AddScoped<ICallDAL, CallDAL>();
            builder.Services.AddScoped<ICallAssignmentDAL, CallAssignmentDAL>();
            builder.Services.AddScoped<ICallService, CallService>();
            builder.Services.AddScoped<ICallAssignmentService, CallAssignmentService>();
            builder.Services.AddScoped<IStrategicZoneBL, StrategicZoneBL>();
            builder.Services.AddScoped<IStrategicZoneDAL, StrategicZoneDAL>();



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
