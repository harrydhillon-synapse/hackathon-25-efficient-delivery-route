using Microsoft.OpenApi.Models;
using Synapse.DeliveryRoutes.Api;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterApp", policy =>
    {
        policy.WithOrigins("http://localhost:52129") 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // This makes enums appear as strings in Swagger
    options.UseInlineDefinitionsForEnums();
    options.SchemaFilter<EnumSchemaFilter>();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFlutterApp");

app.UseAuthorization();

app.MapControllers();

app.Use(async (context, next) =>
{
    await next();

    // Debug: log all response headers
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    foreach (var header in context.Response.Headers)
    {
        logger.LogInformation("{Key}: {Value}", header.Key, header.Value);
    }
});

app.Run();
