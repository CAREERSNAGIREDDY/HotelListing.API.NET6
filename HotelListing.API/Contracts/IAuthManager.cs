using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Contracts
{
    public interface IAuthManager
    {
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);

        //after implement GenerateToken, we are kept comment below line.
        //Task<bool> Login(LoginDto loginDto);

        //For JWT Authentication and Token Generation
        Task<AuthResponseDto> Login(LoginDto loginDto);

        Task<string> CreateRefreshToken();
        Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request);
    }
}

