using CartWebApi.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CartWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;  //UserManager 用於管理使用者身份和執行與使用者相關的操作，例如建立、查找和驗證使用者
        private readonly RoleManager<IdentityRole> _roleManager;  //RoleManager 用於管理角色並執行與角色相關的操作，例如建立、查找和授權角色
        private readonly IConfiguration _configuration;           //IConfiguration 用於訪問應用程式的配置資訊，例如連接字串、API 金鑰等

        public AuthenticateController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel userModel)
        {
            var user = await _userManager.FindByEmailAsync(userModel.Email!); //非空運算符（!）用于明確告诉編譯器，表達式的结果不會為null。

            if (user != null && await _userManager.CheckPasswordAsync(user, userModel.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>    //建立 claims 清單，它將包含在 JWT 中的聲明。聲明是關於用戶的屬性
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                const string CustomClaimTypeRole = "role";

                foreach (var userRole in userRoles)
                {
                    claims.Add(new Claim(CustomClaimTypeRole, userRole));  //迴圈將這些角色添加為JWT的聲明
                }

                var token = CreateToken(claims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token), //將 token 轉為字串表示
                    expiration = token.ValidTo   //JWT的有效截止日期 ，token.ValidTo 是從 JwtSecurityToken 獲取的屬性
                });
            }
            return Unauthorized();
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email!);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Error",
                    Message = "User already exists!"
                });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password!);    //UserManager 自動hashpassword ，密碼需要大+小寫+符號 最少8碼
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }


        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email!);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Error",
                    Message = "User already exists!"
                });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password!);    //UserManager 自動hashpassword
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
            }

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }


        private JwtSecurityToken CreateToken(List<Claim> claims)
        {
            var secretkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                  _configuration.GetValue<string>("JwtSettings:Secret")));

            var credentials = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("JwtSettings:ValidIssuer"),
                audience: _configuration.GetValue<string>("JwtSettings:ValidAudience"),
                expires: DateTime.Now.AddDays(1),
                claims: claims,
                signingCredentials: credentials);

            return token;
        }
    }
}

   

