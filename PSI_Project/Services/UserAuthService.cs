﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PSI_Project.DTO;
using PSI_Project.Models;
using PSI_Project.Repositories;

namespace PSI_Project.Services;

public class UserAuthService
{
    private readonly IConfiguration _config;
    private readonly UserRepository _userRepository;
    
    public UserAuthService(IConfiguration config, UserRepository userRepository)
    {
        _config = config;
        _userRepository = userRepository;
    }

    public User? Authenticate(LoginRequest userData)
    {
        return _userRepository.Find(user => user.Email == userData.Email 
                                            && user.Password == userData.Password).FirstOrDefault();
    }

    public async Task<string> GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtKey"] ?? String.Empty));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id)
        };

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"], 
            _config["Jwt:Audience"], 
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public User? GetUser(HttpContext context)
    {
        var identity = context.User.Identity as ClaimsIdentity;

        if (identity != null)
        {
            var userClaims = identity.Claims;
            var userId = userClaims
                .FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?
                .Value;
            
            return _userRepository.Get(userId);
        }

        return null;
    }
}