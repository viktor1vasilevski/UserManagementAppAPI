﻿namespace Main.DTOs.Auth;

public class UserLoginDTO
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
