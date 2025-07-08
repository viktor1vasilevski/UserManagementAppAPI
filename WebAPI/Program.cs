using Domain.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories;
using Infrastructure.IoC;
using Main.Validations.Auth;
using Microsoft.EntityFrameworkCore;
using WebAPI.Extensions;
using WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(policy => policy.AddPolicy("MyPolicy", builder =>
{
    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(SqlUnitOfWork<>));
builder.Services.AddIoCService();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegisterRequestValidator>(ServiceLifetime.Transient);
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        var password = builder.Configuration["SeedAdmin:Password"];
        AppDbContextSeed.SeedAdminUser(dbContext, password);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during admin user seeding: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("MyPolicy");
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
