using Fortune.Repository;
using Fortune.Repository.Basic;
using Fortune.Repository.DBContext;
using Fortune.Services;
using Microsoft.EntityFrameworkCore;

namespace Fortune
{
    public static class DependancyInjection
    {
        public static void AddDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddDatabase(configuration);

            services.AddScoped(typeof(GenericRepository<>));

            services.AddScoped<UserRepository>();
            services.AddScoped<BookingRepository>();
            services.AddScoped<PlanRepository>();            
            services.AddScoped<MiniGameRepository>();
            services.AddScoped<StaffRepository>();
            services.AddScoped<PackageRepository>();
            services.AddScoped<OrderRepository>();

            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IMiniGameService, MiniGameService>();
            services.AddScoped<IStaffService, StaffService>();            
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped<UserService>();

        }
        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FortuneContext>(options =>
                        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
