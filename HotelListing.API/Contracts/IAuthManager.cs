using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Contracts
{
    public interface IAuthManager
    {
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);

        //after implement GenerateToken, we are kept comment below line.
        //Task<bool> Login(LoginDto loginDto);

        //For JWT Authentication
        Task<AuthResponseDto> Login(LoginDto loginDto);
    }
}

