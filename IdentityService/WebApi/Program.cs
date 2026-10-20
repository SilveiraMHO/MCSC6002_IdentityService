using Application;
using Infrastructure;
using Scalar.AspNetCore;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddOpenApi(options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); }); // This config is to create a Bearer security scheme in the OpenAPI document, so the ScalarAPI can read it and use it for authentication (the Transformer must be created).

// WebApi DI
builder.Services.AddIdentitySettings();

// Infrastructure DI
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddInfrastructureServices();

// Application DI
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Services.GetTokenSettings(builder.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.SeedDatabase();
}

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Identity Service";
    options.DarkMode = true;
    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    options.CustomCss = "";
    options.AddPreferredSecuritySchemes("Bearer")
    .AddHttpAuthentication("Bearer", auth =>
    {
        auth.Token = null;
    });
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
