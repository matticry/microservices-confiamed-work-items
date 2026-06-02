using ItemsTrabajo.Api.Extensions;
using ItemsTrabajo.Application;
using ItemsTrabajo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCors(x => x.AddPolicy("globalCorsApp", policyBuilder =>
{
    policyBuilder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSwaggerDocumentation();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("globalCorsApp");
app.MapControllers();
app.Run();