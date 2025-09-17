using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RubyElektronik.Data;
using RubyElektronik.Models;
using RubyElektronik.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookies for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

// Add JWT service
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "RubyElektronik_Super_Secret_Key_2024_Admin_Panel_Authentication_System_Key_123456789";
var issuer = jwtSettings["Issuer"] ?? "RubyElektronik";
var audience = jwtSettings["Audience"] ?? "RubyElektronikUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add authorization
builder.Services.AddAuthorization();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Ensure database is created and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    context.Database.EnsureCreated();
    
    // Seed roles
    await SeedRoles(roleManager);
    
    // Seed admin user
    await SeedAdminUser(userManager);
}

// Public API Endpoints (herkese açık)
app.MapGet("/api/products", async (ApplicationDbContext context) =>
{
    var products = await context.Products.Where(p => p.IsActive).ToListAsync();
    return Results.Ok(products);
});

app.MapGet("/api/products/{id}", async (int id, ApplicationDbContext context) =>
{
    var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

// Admin-only API Endpoints (sadece admin yetkisi gerekli)
app.MapPost("/api/products", async (RubyElektronik.Models.Product product, ApplicationDbContext context) =>
{
    product.CreatedAt = DateTime.UtcNow;
    context.Products.Add(product);
    await context.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
}).RequireAuthorization("Admin");

app.MapPut("/api/products/{id}", async (int id, RubyElektronik.Models.Product updatedProduct, ApplicationDbContext context) =>
{
    var product = await context.Products.FindAsync(id);
    if (product == null) return Results.NotFound();
    
    product.Name = updatedProduct.Name;
    product.Price = updatedProduct.Price;
    product.Description = updatedProduct.Description;
    product.Category = updatedProduct.Category;
    product.IsActive = updatedProduct.IsActive;
    product.UpdatedAt = DateTime.UtcNow;
    
    await context.SaveChangesAsync();
    return Results.Ok(product);
}).RequireAuthorization("Admin");

app.MapDelete("/api/products/{id}", async (int id, ApplicationDbContext context) =>
{
    var product = await context.Products.FindAsync(id);
    if (product == null) return Results.NotFound();
    
    product.IsActive = false;
    product.UpdatedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();
    
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Admin-only Users API (sadece admin yetkisi gerekli)
app.MapGet("/api/users", async (ApplicationDbContext context) =>
{
    var users = await context.RubyUsers.Where(u => u.IsActive).ToListAsync();
    return Results.Ok(users);
}).RequireAuthorization("Admin");

app.MapGet("/api/users/{id}", async (int id, ApplicationDbContext context) =>
{
    var user = await context.RubyUsers.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    return user is not null ? Results.Ok(user) : Results.NotFound();
}).RequireAuthorization("Admin");

app.MapPost("/api/users", async (RubyElektronik.Models.User user, ApplicationDbContext context) =>
{
    user.CreatedAt = DateTime.UtcNow;
    context.RubyUsers.Add(user);
    await context.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", user);
}).RequireAuthorization("Admin");

app.MapPut("/api/users/{id}", async (int id, RubyElektronik.Models.User updatedUser, ApplicationDbContext context) =>
{
    var user = await context.RubyUsers.FindAsync(id);
    if (user == null) return Results.NotFound();
    
    user.Name = updatedUser.Name;
    user.Email = updatedUser.Email;
    user.UserType = updatedUser.UserType;
    user.CompanyName = updatedUser.CompanyName;
    user.PhoneNumber = updatedUser.PhoneNumber;
    user.Address = updatedUser.Address;
    user.IsActive = updatedUser.IsActive;
    user.UpdatedAt = DateTime.UtcNow;
    
    await context.SaveChangesAsync();
    return Results.Ok(user);
}).RequireAuthorization("Admin");

app.MapDelete("/api/users/{id}", async (int id, ApplicationDbContext context) =>
{
    var user = await context.RubyUsers.FindAsync(id);
    if (user == null) return Results.NotFound();
    
    user.IsActive = false;
    user.UpdatedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();
    
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Admin-only Orders API (sadece admin yetkisi gerekli)
app.MapGet("/api/orders", async (ApplicationDbContext context) =>
{
    var orders = await context.Orders.ToListAsync();
    return Results.Ok(orders);
}).RequireAuthorization("Admin");

app.MapGet("/api/orders/{id}", async (int id, ApplicationDbContext context) =>
{
    var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
}).RequireAuthorization("Admin");

app.MapPost("/api/orders", async (RubyElektronik.Models.Order order, ApplicationDbContext context) =>
{
    order.CreatedAt = DateTime.UtcNow;
    order.TotalPrice = order.UnitPrice * order.Quantity;
    context.Orders.Add(order);
    await context.SaveChangesAsync();
    return Results.Created($"/api/orders/{order.Id}", order);
}).RequireAuthorization("Admin");

app.MapPut("/api/orders/{id}", async (int id, RubyElektronik.Models.Order updatedOrder, ApplicationDbContext context) =>
{
    var order = await context.Orders.FindAsync(id);
    if (order == null) return Results.NotFound();
    
    order.UserId = updatedOrder.UserId;
    order.ProductId = updatedOrder.ProductId;
    order.Quantity = updatedOrder.Quantity;
    order.UnitPrice = updatedOrder.UnitPrice;
    order.TotalPrice = updatedOrder.UnitPrice * updatedOrder.Quantity;
    order.Status = updatedOrder.Status;
    order.Notes = updatedOrder.Notes;
    order.ProductName = updatedOrder.ProductName;
    order.UserName = updatedOrder.UserName;
    order.UpdatedAt = DateTime.UtcNow;
    
    await context.SaveChangesAsync();
    return Results.Ok(order);
}).RequireAuthorization("Admin");

app.MapDelete("/api/orders/{id}", async (int id, ApplicationDbContext context) =>
{
    var order = await context.Orders.FindAsync(id);
    if (order == null) return Results.NotFound();
    
    context.Orders.Remove(order);
    await context.SaveChangesAsync();
    
    return Results.NoContent();
}).RequireAuthorization("Admin");

// Service Records API
// Public endpoints (herkese açık)
app.MapPost("/api/servicerecords", async (RubyElektronik.Models.ServiceRecord serviceRecord, ApplicationDbContext context) =>
{
    serviceRecord.CreatedAt = DateTime.UtcNow;
    context.ServiceRecords.Add(serviceRecord);
    await context.SaveChangesAsync();
    return Results.Created($"/api/servicerecords/{serviceRecord.Id}", serviceRecord);
});

// Admin-only endpoints (sadece admin yetkisi gerekli)
app.MapGet("/api/servicerecords", async (ApplicationDbContext context) =>
{
    var serviceRecords = await context.ServiceRecords.Where(s => s.IsActive).ToListAsync();
    return Results.Ok(serviceRecords);
}).RequireAuthorization("Admin");

app.MapGet("/api/servicerecords/{id}", async (int id, ApplicationDbContext context) =>
{
    var serviceRecord = await context.ServiceRecords.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    return serviceRecord is not null ? Results.Ok(serviceRecord) : Results.NotFound();
}).RequireAuthorization("Admin");

// Admin-only endpoints (sadece admin yetkisi gerekli)
app.MapGet("/api/servicerecords/all", async (ApplicationDbContext context) =>
{
    var serviceRecords = await context.ServiceRecords.ToListAsync();
    return Results.Ok(serviceRecords);
}).RequireAuthorization("Admin");

app.MapPut("/api/servicerecords/{id}", async (int id, RubyElektronik.Models.ServiceRecord updatedServiceRecord, ApplicationDbContext context) =>
{
    var serviceRecord = await context.ServiceRecords.FindAsync(id);
    if (serviceRecord == null) return Results.NotFound();
    
    serviceRecord.Ad = updatedServiceRecord.Ad;
    serviceRecord.Soyad = updatedServiceRecord.Soyad;
    serviceRecord.UserType = updatedServiceRecord.UserType;
    serviceRecord.FirmaAdi = updatedServiceRecord.FirmaAdi;
    serviceRecord.TelefonNumarasi = updatedServiceRecord.TelefonNumarasi;
    serviceRecord.UrunTuru = updatedServiceRecord.UrunTuru;
    serviceRecord.ArizaAciklamasi = updatedServiceRecord.ArizaAciklamasi;
    serviceRecord.IsActive = updatedServiceRecord.IsActive;
    serviceRecord.UpdatedAt = DateTime.UtcNow;
    
    await context.SaveChangesAsync();
    return Results.Ok(serviceRecord);
}).RequireAuthorization("Admin");

app.MapDelete("/api/servicerecords/{id}", async (int id, ApplicationDbContext context) =>
{
    var serviceRecord = await context.ServiceRecords.FindAsync(id);
    if (serviceRecord == null) return Results.NotFound();
    
    serviceRecord.IsActive = false;
    serviceRecord.UpdatedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();
    
    return Results.NoContent();
}).RequireAuthorization("Admin");

app.MapDelete("/api/servicerecords/{id}/permanent", async (int id, ApplicationDbContext context) =>
{
    var serviceRecord = await context.ServiceRecords.FindAsync(id);
    if (serviceRecord == null) return Results.NotFound();
    
    context.ServiceRecords.Remove(serviceRecord);
    await context.SaveChangesAsync();
    
    return Results.NoContent();
}).RequireAuthorization("Admin");

app.MapPut("/api/servicerecords/{id}/complete", async (int id, ApplicationDbContext context) =>
{
    var serviceRecord = await context.ServiceRecords.FindAsync(id);
    if (serviceRecord == null) return Results.NotFound();
    
    serviceRecord.IsActive = false;
    serviceRecord.UpdatedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();
    
    return Results.Ok(serviceRecord);
}).RequireAuthorization("Admin");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Seeding methods
async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "User" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
{
    var adminEmail = "admin@rubyelektronik.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
