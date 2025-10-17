using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NexusGram.Data;
using NexusGram.Services;
using NexusGram.Hubs;
using System.Security.Cryptography; // Hata ayıklama için kullanılabilir

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS Yapılandırması
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5255")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// SignalR ve Diğer Servisler
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// Swagger/OpenAPI Yapılandırması (JWT Desteği ile)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NexusGram API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Veritabanı (DbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// --------------------------------------------------------------------------------
// 🚨 KRİTİK DÜZELTME: JWT Doğrulama (Authentication) Ayarları
// --------------------------------------------------------------------------------

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 1. Anahtar Ayarını appsettings.json'daki 'Jwt:Key' ile eşleştiriyoruz.
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? 
                                         throw new InvalidOperationException("JWT Secret Key is not configured. Check 'Jwt:Key' in appsettings.json."));
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Tüm kontrolleri aktif ediyoruz
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            // Değerleri appsettings.json'dan çekiyoruz
            ValidIssuer = builder.Configuration["Jwt:Issuer"], 
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromSeconds(5)
        };
        options.MapInboundClaims = false;
        
        // Hata ayıklama için tokenın ne zaman expired olduğunu görmenizi sağlayabilir.
        // options.Events = new JwtBearerEvents
        // {
        //     OnAuthenticationFailed = context =>
        //     {
        //         if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
        //         {
        //             context.Response.Headers.Add("Token-Expired", "true");
        //         }
        //         return Task.CompletedTask;
        //     }
        // };
    });

// --------------------------------------------------------------------------------
// DI Servisleri
// --------------------------------------------------------------------------------

builder.Services.AddAuthorization(); // Yetkilendirme servisi (AddAuthentication'dan sonra olmalı)

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IFollowService, FollowService>();

// --------------------------------------------------------------------------------
// Uygulama Middleware Pipeline
// --------------------------------------------------------------------------------

var app = builder.Build();

app.UseCors("AllowReactApp");

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Kimlik doğrulama ve yetkilendirme middleware'leri bu sırayla olmalıdır.
app.UseAuthentication();
app.UseAuthorization();

// SignalR Hubs
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();