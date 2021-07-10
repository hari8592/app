using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

using API.Entities;
using API.Data;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOS;
using AutoMapper;

namespace API.Controllers
{
    //[Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository,IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }
        //api//user/s
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            //var users = await _userRepository.GetUserByUsernameAsync(username);
            //return _mapper.Map<MemberDTO>(users);
            return  await _userRepository.GetMemberAsync(username);
        }

    }
}