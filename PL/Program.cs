var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDalServices(builder.Configuration);
builder.Services.AddBllServices();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Resume & Vacancy API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new()
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Enter your JWT token (without the 'Bearer ' prefix).",
    });
    options.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            []
        },
    });
});

var app = builder.Build();

app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallback(context =>
{
    context.Response.StatusCode  = StatusCodes.Status404NotFound;
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsJsonAsync(new ErrorResponse("The requested endpoint was not found."));
});

app.Run();
