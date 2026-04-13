using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LiveExpert.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=../LiveExpert.API/liveexpert.db"));
    }).Build();

using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

var subjects = await dbContext.Subjects.ToListAsync();
Console.WriteLine($"Total Subjects: {subjects.Count}");
foreach(var s in subjects) {
    Console.WriteLine($"ID: {s.Id}, Name: {s.Name}, IsActive: {s.IsActive}");
}
