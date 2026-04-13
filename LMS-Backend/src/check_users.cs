using Microsoft.EntityFrameworkCore;
using LiveExpert.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) => {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=../LiveExpert.API/liveexpert.db"));
    }).Build();

using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var users = await context.Users.Include(u => u.TutorProfile).ToListAsync();
    
    Console.WriteLine($"Found {users.Count} users:");
    foreach (var user in users)
    {
        Console.WriteLine($"- {user.Email} (Role: {user.Role}, HasProfile: {user.TutorProfile != null})");
    }
}
