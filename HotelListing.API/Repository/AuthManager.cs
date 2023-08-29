using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private ApiUser _user;
        private readonly Logger<AuthManager> _logger;

        private const string _loginProvider = "HotelListingApi";
        private const string _refreshtoken = "RefreshToken";

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration, Logger<AuthManager> logger)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<string> CreateRefreshToken()
        {
            await _userManager.RemoveAuthenticationTokenAsync(_user, _loginProvider, _refreshtoken);
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, _loginProvider, _refreshtoken);
            var result = await _userManager.SetAuthenticationTokenAsync(_user, _loginProvider, _refreshtoken, newRefreshToken);
            return newRefreshToken;
        }

        #region after implement GenerateToken, we are kept comment below code.
        //public async Task<bool> Login(LoginDto loginDto)
        //{
        //    bool isValidUser = false;
        //    try
        //    {
        //        //If the user object comes back as null, this will lead to a null exception in the CheckPasswordAsync()
        //        //method.We can refactor like this:
        //        #region Fix the CheckPasswordAsyn() issue.
        //        //var user = await _userManager.FindByEmailAsync(loginDto.Email);
        //        //isValidUser = await _userManager.CheckPasswordAsync(user, loginDto.Pasword);
        //        #endregion

        //        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        //        if (user == null)
        //        {
        //            return default;
        //        }

        //        isValidUser = await _userManager.CheckPasswordAsync(user, loginDto.Pasword);
        //        if (!isValidUser)
        //        {
        //            return default;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    return isValidUser;
        //}
        #endregion

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            _logger.LogInformation($"Looking for user with email {loginDto.Email}");
            _user = await _userManager.FindByEmailAsync(loginDto.Email);
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDto.Pasword);
            if (_user == null || isValidUser == false)
            {
                _logger.LogWarning($"User with email {loginDto.Email} was not found");
                return null;
            }

            var token = await GenerateToken();
            _logger.LogInformation($"Token generation user with email {loginDto.Email} | Token : {token}");

            return new AuthResponseDto
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken=await CreateRefreshToken()//It has fixed in 59th Video and 62nd vedio also fixed the issue. 
            };
        }
        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = _mapper.Map<ApiUser>(userDto);
            _user.UserName = userDto.Email;

            var result = await _userManager.CreateAsync(_user, userDto.Pasword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }
            return result.Errors;
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            _user = await _userManager.FindByNameAsync(username);

            if (_user == null || _user.Id != request.UserId)
            {
                return null;
            }

            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshtoken, request.RefreshToken);

            if (isValidRefreshToken)
            {
                var token = await GenerateToken();

                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }
            await _userManager.UpdateSecurityStampAsync(_user);
            return null;
        }

        private async Task<string> GenerateToken()
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            var userClaims = await _userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id),
            }
            .Union(userClaims).Union(roleClaims);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
