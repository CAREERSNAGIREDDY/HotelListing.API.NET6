using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager)
        {
            this._mapper = mapper;
            this._userManager = userManager;
        }

        public async Task<bool> Login(LoginDto loginDto)
        {
            bool isValidUser = false;
            try
            {
                //If the user object comes back as null, this will lead to a null exception in the CheckPasswordAsync()
                //method.We can refactor like this:
                #region Fix the CheckPasswordAsyn() issue.
                //var user = await _userManager.FindByEmailAsync(loginDto.Email);
                //isValidUser = await _userManager.CheckPasswordAsync(user, loginDto.Pasword);
                #endregion

                var user=await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return default;
                }

                isValidUser = await _userManager.CheckPasswordAsync(user, loginDto.Pasword);
                if (!isValidUser)
                {
                    return default;
                }
            }
            catch (Exception)
            {

            }
            return isValidUser;
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            var user = _mapper.Map<ApiUser>(userDto);
            user.UserName = userDto.Email;

            var result = await _userManager.CreateAsync(user, userDto.Pasword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            return result.Errors;
        }
    }
}
