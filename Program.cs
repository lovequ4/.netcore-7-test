using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CartWebApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace CartWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigurationManager _configuration = builder.Configuration;
            var secret = _configuration.GetValue<string>("JwtSettings:Secret");


            // Add services to the container.
            builder.Services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Database"))
             );

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApiDbContext>()
                .AddDefaultTokenProviders();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            })
              .AddJwtBearer(options =>
              {
                  // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
                  options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      // 透過這項宣告，就可以從 "NAME" 取值
                      NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                      // 透過這項宣告，就可以從 "Role" 取值，並可讓 [Authorize] 判斷角色
                      RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                      // 驗證 Issuer (一般都會)
                      ValidateIssuer = true,
                      ValidIssuer = _configuration.GetValue<string>("JwtSettings:ValidIssuer"),

                      // 驗證 Audience (通常不太需要)
                      ValidateAudience = false,
                      //ValidAudience = = _configuration.GetValue<string>("JwtSettings:ValidAudience"),

                      // 驗證 Token 的有效期間 (一般都會)
                      ValidateLifetime = true,

                      // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                      ValidateIssuerSigningKey = false,

                      // 應該從 IConfiguration 取得
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                  };
              });
        
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>   // Swagger 測試網頁開啟 Token 的功能
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtDemo", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                            },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseAuthentication();
 

            app.MapControllers();

            app.Run();
        }
    }
}