using DataCommon;
using DataCommon.Models.Documents;
using DataService.Repository;
using DataService.Repository.Interfaces.Documents;
using DataService.Services;
using DataService.Services.Interfaces.Documents;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var Configuration = builder.Configuration;

Console.WriteLine(Configuration["Jwt:Issuer"]);
builder.Services.AddSingleton<IDbClient, MongoDbClient>();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
{
    var enumConverter = new JsonStringEnumConverter();
    opts.JsonSerializerOptions.Converters.Add(enumConverter);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; //TO

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Configuration["Jwt:Issuer"],
        ValidAudience = Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.FromMinutes(1)// 1 minuta tolerancji
    };
});

var mongoConfiguration = builder.Configuration.GetSection("MongoDB").Get<MongoDatabaseSettings>();
builder.Services.AddIdentityCore<ApplicationUser>().AddRoles<ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, ObjectId>(mongoConfiguration.ConnectionURI, mongoConfiguration.DatabaseName);

//Repositories
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<IIgnoredRepository, IgnoredRepository>();
builder.Services.AddTransient<IPostRepository, PostRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IFollowersRepository, FollowerRepository>();
builder.Services.AddTransient<ISharePostRepository, SharePostRepository>();
builder.Services.AddTransient<ILikesRepositiry,LikesRepositiry>();

//Services
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITagService, TagService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ISharePostService, SharePostService>();
builder.Services.AddTransient<ILikeService, LikeService>();
builder.Services.AddTransient<IPostService, PostService>();
builder.Services.AddTransient<IFollowService, FollowService>();
builder.Services.AddTransient<IFileService, FileService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

#region app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use((ctx,next) =>
{
    Console.WriteLine($"\n\t\t Host:{ctx.Request.Host.Host} - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture)} - {ctx.Request.Method}:{ctx.Request.Path}\t {ctx.Request.Headers["X-MyHeader"]}");
    return next();
});
app.UseHttpsRedirection();

app.UseAuthentication();



app.UseAuthorization();


app.MapControllers();

app.Run();
#endregion
