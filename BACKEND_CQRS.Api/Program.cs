using BACKEND_CQRS.Application;
using BACKEND_CQRS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ------------------------- CORS -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") // Angular dev server
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ------------------------- Services -------------------------
// Application layer (AutoMapper, MediatR, FluentValidation)
builder.Services.AddApplicationServices();

// Infrastructure layer (DbContext, Repositories, Services, Logging)
builder.Services.AddPersistenceServices(builder.Configuration);

// ------------------------- Controllers -------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ------------------------- Swagger -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------------- Middleware -------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");

app.UseAuthorization();

app.MapControllers();

app.Run();
