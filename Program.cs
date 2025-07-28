using Microsoft.EntityFrameworkCore;
using MyStockAPIs.Services;
using TestsAPiss.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(); // make Swagger work
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDBContext>(options =>{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//main builder
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<HttpLoggingMiddleware>(); // Added the custom middleware to the pipeline//To log only application-specific requests (skip Swagger, static files)?
//Move/Place this HttpLoggingMiddleware after UseStaticFiles() and UseSwaggerUI()
app.UseAuthorization();
app.MapControllers(); /// makes Swagger work
app.Run();
