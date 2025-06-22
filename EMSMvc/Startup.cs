using EMSMvc.Core.Application.Interfaces;
using EMSMvc.Core.Application.Services;
using EMSMvc.Infrastructure.Config;
using EMSMvc.Infrastructure.Implementations;

namespace EMSMvc
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpClient("EMSWebApi", client =>
            {
                //client.BaseAddress = new Uri("https://waqargondal-001-site1.qtempurl.com/");  // Replace with your Web API URL
                client.BaseAddress = new Uri("https://localhost:44364/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.Configure<ApiSettings>(Configuration.GetSection("ApiSettings"));


            services.AddHttpClient<IApiService, ApiService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<AuthService>();
            services.AddScoped<EmployeeService>();
            services.AddScoped<UserManagementService>();
            services.AddScoped<AttendanceService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<LeaveService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<NoticeService>();

            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "EMSMvcCookieAuth";
                options.DefaultChallengeScheme = "Google";
            })
            .AddCookie("EMSMvcCookieAuth", options =>
            {
                options.LoginPath = "/Auth/Login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            })
            .AddGoogle(options =>
            {
                options.ClientId = Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];

                options.SaveTokens = true;
            });
        }

        public void Configure(WebApplication app)
        {
            if (!Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}");
        }
    }
}
