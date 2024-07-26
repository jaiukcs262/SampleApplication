using Api.DTOs;
using Api.Services;
using API.Data;
using API.Dtos;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers
{
    public class AccountController(DataContext dataContext , ITokenService tokenService) : BaseController
    {
        [HttpPost]
        [Route("RegisterUser")]
        public async Task<ActionResult<UserDTO>> RegisterUser([FromBody] RegisterUserDTO registerUser)
        {
            if (registerUser == null)
            {
                return new BadRequestResult();
            }
            var user = await CheckUser(registerUser.UserName);
            if (user != null)
            {
                return new BadRequestObjectResult("user already Exist");
            }
            var hmac = new HMACSHA512();
            await dataContext.Users.AddAsync(new API.Entities.AppUser { UserName = registerUser.UserName, PasswordSalt = hmac.Key, PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUser.Password)) });
            await dataContext.SaveChangesAsync();
            user = await CheckUser(registerUser.UserName);
            return new UserDTO { UserName = registerUser.UserName, Token = tokenService.CreateToken(user) };
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<UserDTO>> Login([FromBody] LoginDTO loginUser)
        {
            if (loginUser == null)
            {
                return new BadRequestResult();
            }
            var user = await dataContext.Users.FirstOrDefaultAsync(x => x.UserName.ToLower() == loginUser.UserName.ToLower());
            if (user == null)
            {
                return Unauthorized("User not found");
            }
            var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginUser.Password));
            if (computedHash.Length != user.PasswordHash.Length)
            {
                return Unauthorized("User name or password incorrect");
            }
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("User name or password incorrect");
                }
            }
            return new UserDTO
            {
                UserName = loginUser.UserName,
                Token = tokenService.CreateToken(user)
            };
        }

        private async Task<AppUser?> CheckUser(string userName)
        {
            int a = 12, b = 24;
            int c = a + b;
            return await dataContext.Users.FirstOrDefaultAsync(x => x.UserName.ToLower() == userName.ToLower());
        }
    }
}