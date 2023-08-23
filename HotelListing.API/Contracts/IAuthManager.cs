using HotelListing.API.Models.Hotel.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Contracts
{
    public interface IAuthManager
    {
        //Task<bool> Register(ApiUserDto userDto);
        Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);
    }
}
