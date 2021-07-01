using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;

using API.Data;
using API.Entities;
using API.DTOS;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context,ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName)) return BadRequest("UserName is already taken");

            //once object used done it will call dispose so here we have used using
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
        private async Task<bool> UserExists(string UserName)
        {
            return await _context.Users.AnyAsync(x => x.UserName == UserName.ToLower());
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDTo)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDTo.UserName);

            if (user == null) return Unauthorized("Invalid UserName");

            //taken out passwordsalt from db i.e encrypted pwd
            using var hmac = new HMACSHA512(user.PasswordSalt);

            //applied passwordhash to entered pwd
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTo.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if(computedHash[i]!=user.PasswordHash[i])   return Unauthorized("Invalid Password");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }
    }
}
