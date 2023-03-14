using AuthProject.Helpers;
using Diplomka.Models;
using Diplomka.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(ApplicationDbContext dbContext,
                             UserManager<IdentityUser> userManager,
                             RoleManager<IdentityRole
                                 > roleManager,
                             SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }
        [HttpPost]
        [Route("Register")]
        public async Task Register([FromBody] RegisterViewModel model)
        {
            await _roleManager.CreateAsync(new IdentityRole("Student"));

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {

                Response.ContentType = "application/json";
                Response.StatusCode = 409;
                await Response.WriteAsync("Email exists");
                return;
            }


            var user = new IdentityUser();
            user.Email = model.Email;
            user.UserName = model.Email;
            await _userManager.CreateAsync(user, model.Password);


            await _userManager.AddToRoleAsync(user, "Student");
            var person = new User();
            person.IdentityUser = user;
            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            await _dbContext.Users.AddAsync(person);
            await _dbContext.SaveChangesAsync(default);
            //await Token(model.Email);
        }


        [HttpPost]
        [Route("Login")]
        public async Task Login([FromBody] LoginViewModel model)
        {
            var result =
                await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (!result.Succeeded)
            {

                Response.StatusCode = 401;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }
            await Token(model.Username);
        }

        [Authorize]
        [HttpGet("CheckAuth")]
        public async Task<IActionResult> CheckAuth()
        {
            return Ok("Authorized");
        }
        private async Task Token(string email)
        {
            var identity = GetIdentity(email);
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name,
            };
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }
        private ClaimsIdentity GetIdentity(string login)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, login),
                new Claim(ClaimTypes.Role, "User")
            };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }
    }
}
