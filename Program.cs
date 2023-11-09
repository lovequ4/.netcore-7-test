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
                  // �����ҥ��ѮɡA�^�����Y�|�]�t WWW-Authenticate ���Y�A�o�̷|��ܥ��Ѫ��Բӿ��~��]
                  options.IncludeErrorDetails = true; // �w�]�Ȭ� true�A���ɷ|�S�O����

                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      // �z�L�o���ŧi�A�N�i�H�q "NAME" ����
                      NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                      // �z�L�o���ŧi�A�N�i�H�q "Role" ���ȡA�åi�� [Authorize] �P�_����
                      RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                      // ���� Issuer (�@�볣�|)
                      ValidateIssuer = true,
                      ValidIssuer = _configuration.GetValue<string>("JwtSettings:ValidIssuer"),

                      // ���� Audience (�q�`���ӻݭn)
                      ValidateAudience = false,
                      //ValidAudience = = _configuration.GetValue<string>("JwtSettings:ValidAudience"),

                      // ���� Token �����Ĵ��� (�@�볣�|)
                      ValidateLifetime = true,

                      // �p�G Token ���]�t key �~�ݭn���ҡA�@�볣�u��ñ���Ӥw
                      ValidateIssuerSigningKey = false,

                      // ���ӱq IConfiguration ���o
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                  };
              });
        
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>   // Swagger ���պ����}�� Token ���\��
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