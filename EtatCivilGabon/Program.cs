using EtatCivilGabon.Data;
using EtatCivilGabon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Base de données ───────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. ASP.NET Core Identity ─────────────────────────────────────────────────
builder.Services.AddIdentity<Utilisateur, IdentityRole>(options =>
{
    // Politique de mots de passe
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength         = 8;

    // Verrouillage de compte après tentatives échouées
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // Email unique obligatoire
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── 3. Cookie d'authentification ─────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath          = "/Compte/Connexion";
    options.LogoutPath         = "/Compte/Deconnexion";
    options.AccessDeniedPath   = "/Compte/AccesRefuse";
    options.ExpireTimeSpan     = TimeSpan.FromHours(8);
    options.SlidingExpiration  = true;
});

// ── 4. Services métier ───────────────────────────────────────────────────────
builder.Services.AddScoped<EtatCivilGabon.Services.NumeroSuiviGenerator>();
builder.Services.AddScoped<EtatCivilGabon.Services.IDemandeService,
                           EtatCivilGabon.Services.DemandeService>();

// Configuration MailKit (section MailSettings dans appsettings.json)
builder.Services.Configure<EtatCivilGabon.Services.MailSettings>(
    builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<EtatCivilGabon.Services.INotificationService,
                           EtatCivilGabon.Services.NotificationService>();

// ── 5. MVC ───────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── 6. Initialisation de la base de données au démarrage ─────────────────────
using (var scope = app.Services.CreateScope())
{
    var services     = scope.ServiceProvider;
    var context      = services.GetRequiredService<AppDbContext>();
    var userManager  = services.GetRequiredService<UserManager<Utilisateur>>();
    var roleManager  = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Applique les migrations automatiquement
    await context.Database.MigrateAsync();

    // Crée les rôles et l'admin par défaut
    await DbInitializer.InitialiserAsync(services, userManager, roleManager);
}

// ── 7. Pipeline HTTP ─────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ── 8. Routes ────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
