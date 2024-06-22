
using Microsoft.EntityFrameworkCore;
using PaginationDemo.Models;
using PaginationDemo.Utilities.Repositories;
using PaginationDemo.Utilities;
using PaginationDemo.Service;
using PaginationDemo.Utilities.Implementations;

namespace PaginationDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddScoped(typeof(IPaginationRepository<>), typeof(PaginationImplementation<>));
            builder.Services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilderImplementation<>));
            builder.Services.AddScoped<ExternalPatronPage>();
            builder.Services.AddScoped<ExternalPatronService>();
            builder.Services.AddDbContext<PaginationDemo.Models.ExternalPatronDbContext>(options =>
                    options.UseMySql(
                        builder.Configuration.GetConnectionString("CRUDConnection"),
                        new MySqlServerVersion(new Version(8, 0, 32))
                    )
            );

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Seed database with initial data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<PaginationDemo.Models.ExternalPatronDbContext>();
                SeedData.Initialize(context);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
