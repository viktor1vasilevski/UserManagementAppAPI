﻿namespace Main.Requests.User;

public class UserRequest
{
    public string? Username { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
