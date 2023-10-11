using Amazon.Runtime.Internal;
using DataCommon;
using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.DTO.Utils;
using DataService.Repository;
using DataService.Repository.Interfaces.Documents;
using DataService.Services;
using DataService.Services.Interfaces.Documents;
using GraphQL.Auth;
using GraphQL.Errors;
using GraphQL.Follows;
using GraphQL.Likes;
using GraphQL.Posts;
using GraphQL.SharePosts;
using GraphQL.Tags;
using GraphQL.TypesUtils;
using GraphQL.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDbClient, MongoDbClient>();
var Configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


#region Auth

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; //TODO

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
builder.Services.AddAuthorization();
var mongoConfiguration = builder.Configuration.GetSection("MongoDB").Get<MongoDatabaseSettings>();
builder.Services.AddIdentityCore<ApplicationUser>().AddRoles<ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, ObjectId>(mongoConfiguration.ConnectionURI, mongoConfiguration.DatabaseName);
#endregion
builder.Services.AddHttpContextAccessor();
#region Repositories
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<IIgnoredRepository, IgnoredRepository>();
builder.Services.AddTransient<IPostRepository, PostRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IFollowersRepository, FollowerRepository>();
builder.Services.AddTransient<ISharePostRepository, SharePostRepository>();
builder.Services.AddTransient<ILikesRepositiry, LikesRepositiry>();
#endregion

#region Services
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ITagService, TagService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ISharePostService, SharePostService>();
builder.Services.AddTransient<ILikeService, LikeService>();
builder.Services.AddTransient<IPostService, PostService>();
builder.Services.AddTransient<IFollowService, FollowService>();
builder.Services.AddTransient<IFileService, FileService>();
#endregion

#region AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
#endregion

#region GraphQL
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddMutationConventions(applyToAllMutations: false)
    .AddType<UploadType>()

    .AddType<TagInputType>()
    .AddType<TagType>()
    .AddType<PageWrapperTagType>()

    .AddType<ResultBoolType>()

    .AddType<UserType>()
    .AddType<PageWrapperUserType>()
    .AddType<UserInputType>()
    .AddInputObjectType<UpdateUser>(d=>d.Description("Update User Type"))

    .AddType<SharePostType>()
    .AddType<PageWrapperSharePostType>()
    .AddType<SharePostInputType>()

    .AddType<PostType>()
    .AddType<PageWrapperPostType>()
    .AddType<PostInputType>()

    .AddType<LikeType>()
    .AddType<PageWrapperLikeType>()
    .AddType<LikeInputType>()

    .AddErrorFilter<ErrorNotFound>()
    .AddErrorFilter<AuthError>()
    .AddErrorFilter<MainErrorFilter>()

    .AddType<FollowType>()

    .AddQueryType(d => {
        d.Name(OperationTypeNames.Query);
        d.Description(OperationTypeNames.Query);
    })
        .AddTypeExtension<TagQueriesExtension>()
        .AddTypeExtension<SharePostQueriesExtension>()
        .AddTypeExtension<PostQueriesExtension>()
        .AddTypeExtension<LikeQueriesExtension>()
        .AddTypeExtension<FollowQueriesExtension>()
        .AddTypeExtension<UserQueriesExtension>()

    .AddMutationType(d => {
            d.Name(OperationTypeNames.Mutation);
            d.Description(OperationTypeNames.Mutation);
        })
        .AddTypeExtension<TagMutationsExtension>()
        .AddTypeExtension<AuthMutationsExtension>()
        .AddTypeExtension<SharePostMutationsExtension>()
        .AddTypeExtension<PostMutationsExtension>()
        .AddTypeExtension<LikeMutationsExtension>()
        .AddTypeExtension<FollowMutationsExtension>()
        .AddTypeExtension<UserMutationsExtension>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async(ctx, next) =>
{
    Console.WriteLine($"\n\t\t Host:{ctx.Request.Host.Host} - {DateTime.Now} - {ctx.Request.Method}:{ctx.Request.Path}");

    if (ctx.Request.Headers.UserAgent.ToString().Contains("PostmanRuntime"))
    {
        Console.WriteLine(string.Concat(Enumerable.Repeat("-", 20)));
        string rawContent = string.Empty;
        using (var reader = new StreamReader(ctx.Request.Body,
                      encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
        {
            rawContent = await reader.ReadToEndAsync();
        }
        Console.WriteLine(rawContent);
        Console.WriteLine(string.Concat(Enumerable.Repeat("-", 20)));
    }
    await next();
});

//app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

app.Run();
