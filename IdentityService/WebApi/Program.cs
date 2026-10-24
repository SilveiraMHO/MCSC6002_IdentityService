using Application;
using Infrastructure;
using Scalar.AspNetCore;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.AddMutualTls(builder.Configuration); // Only the API Gateway can connect.

builder.Services.AddControllers();

builder.Services.AddOpenApi(options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); }); // Bearer scheme for Scalar.

builder.Services.AddIdentitySettings();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddInfrastructureServices();

builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Services.GetTokenSettings(builder.Configuration));

var app = builder.Build();

if (app.Configuration.GetValue<bool>("SeedDatabase")) // Migrations and seed users; docker-compose sets it.
{
    await app.SeedDatabase();
}

if (app.Environment.IsDevelopment())
{
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
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
