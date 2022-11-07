using WebApp.CloudApi.Class;
using WebApp.CloudApi.EfCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using WebApp.CloudApi.Helper;
using Amazon.S3;
using WebApp.CloudApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("sample.json",
                       optional: true,
                       reloadOnChange: true);
});

// Add services to the container.
// Add services to the container.

GlobalData.Application.Add(new KeyValuePair<string, string>("Database", builder.Configuration["Database"]));
GlobalData.Application.Add(new KeyValuePair<string, string>("DatabaseName", builder.Configuration["DatabaseName"]));
GlobalData.Application.Add(new KeyValuePair<string, string>("DatabasePort", builder.Configuration["DatabasePort"]));
GlobalData.Application.Add(new KeyValuePair<string, string>("MasterUsername", builder.Configuration["MasterUsername"]));
GlobalData.Application.Add(new KeyValuePair<string, string>("MasterPassword", builder.Configuration["MasterPassword"]));
GlobalData.Application.Add(new KeyValuePair<string, string>("BucketName", builder.Configuration["BucketName"]));
// builder.Services.AddDbContext<EF_DataContext>();
// builder.Services.AddDbContext<EF_DataContext>(options =>
// {
//     options.UseNpgsql("Host=testdb1.cbd0o3qojchd.us-east-1.rds.amazonaws.com;Database=postgrestest;Port=5432;Username=postgres;Password=postgres;");
// });
// builder.Services.AddDbContext<EF_DataContext>(options =>
// {
//     options.UseNpgsql("Host=testdb1.cbd0o3qojchd.us-east-1.rds.amazonaws.com;Database=postgrestest;Port=5432;Username=postgres;Password=postgres;");
// });
string connectionString = $"Host={GlobalData.Application.Where(s => s.Key == "Database").FirstOrDefault().Value};Database={GlobalData.Application.Where(s => s.Key == "DatabaseName").FirstOrDefault().Value};Port={GlobalData.Application.Where(s => s.Key == "DatabasePort").FirstOrDefault().Value};Username={GlobalData.Application.Where(s => s.Key == "MasterUsername").FirstOrDefault().Value};Password={GlobalData.Application.Where(s => s.Key == "MasterPassword").FirstOrDefault().Value};";
Console.WriteLine(connectionString);
builder.Services.AddDbContext<EF_DataContext>(options =>
{
    options.UseNpgsql(connectionString);
});
// builder.Services.AddDbContext<EF_DataContext>(
//     o => o.UseNpgsql("Server=" + builder.Configuration["Database"] +";Database=" + builder.Configuration["DatabaseName"]
//      +";Port=" + builder.Configuration["DatabasePort"] +";UserId=" + builder.Configuration["MasterUsername"] +
//     ";Password=" + builder.Configuration["MasterPassword"] +";")
// );

builder.Services.AddControllers();
builder.Services.AddSingleton<ApplicationInstance>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services
.AddAuthentication()
.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });
builder.Services.AddScoped<IDbHelper, DbHelper>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
});

builder.Services.AddDefaultAWSOptions(new Amazon.Extensions.NETCore.Setup.AWSOptions
{
    Profile = builder.Configuration["AwsProfile"],
    Region = RegionEndpoint.USIsoEast1
});
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Cloud API",
        Description = "An ASP.NET Core Web API for Cloud Project",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "License",
            Url = new Uri("https://example.com/license")
        }
    });

    // Set the comments path for the Swagger JSON and UI.
    // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cloud API V1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.Run();
}
else
{
    app.Run("http://0.0.0.0:8080");
}
