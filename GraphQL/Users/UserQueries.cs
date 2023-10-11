using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.Exceptions;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using HotChocolate.Types;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace GraphQL.Users
{
    public class UserQueries
    {
        public async Task<PageWrapper<UserDTO>> Get([Service] IUserService userService,int size = 10, int page = 0)
        {
            var result = await userService.GetAll(size, page);
            await Console.Out.WriteLineAsync("twe");
            return result;
        }

        public async Task<UserDTO> GetById([Service] IUserService userService, string id)
        {
            var result = await userService.GetById(id);
            return result;
        }

        public async Task<IEnumerable<Report>> GetReports([Service] IUserService userService, string userId)
        {
            try
            {
                var result = await userService.GetReportsByUserId(userId);
                return result;
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<PageWrapper<UserDTO>> Search([Service] IUserService userService,string q, int size = 10, int page = 0)
        {
            var sort = Builders<ApplicationUser>.Sort.Ascending(u => u.Surename).Ascending(u => u.Name);
            var result = await userService.SearchPageable(q, sort, size, page);
            return result;
        }

        public async Task<IEnumerable<Warning>> GetWarnings([Service] IUserService userService, string userId)
        {
            try
            {
                var result = await userService.GetWarningsByUserId(userId);
                return result;
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<PageWrapper<UserDTO>> GetBlockUserList([Service] IUserService userService,string userId, int size = 10, int page = 0)
        {
            try
            {
                var result = await userService.GetBlockUserByUserIdPageableSort(userId, size, page);

                return result;
            }
            catch (UserNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (UserTransactionException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public string Greeting(string name)
        {
            return $"Hello World, my name is {name}";
        }
    }
    public class UserQueriesExtension : ObjectTypeExtension<UserQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<UserQueries> descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor.Field(u => u.Get(default!, default!, default!))
                .Name("users")
                .Authorize(new[] { UserRoles.Admin })
                .Description("Get paginated users");


            descriptor.Field(u => u.GetById( default!, default!))
                .Name("user")
                .Description("Get user");

            descriptor.Field(u => u.GetReports(default!, default!))
                .Authorize()
                .Description("Get reports for user");

            descriptor.Field(u => u.Search(default!, default!, default!, default!))
               .Name("searchUsers")
               .Description("Search users");

            descriptor.Field(u => u.GetWarnings( default!, default!))
                .Authorize()
                .Description("Get warnings for users");

            descriptor.Field(u => u.GetBlockUserList(default!, default!, default!, default!))
               .Authorize()
               .Description("Get My Blocked Users");

            descriptor.Field("sayMyName")
                //.Authorize(new[] { UserRoles.Admin })
                .Argument("username", a => a.Type<NonNullType<StringType>>())
                .Resolve(ctx =>
                {
                    var s = ctx.Service<IFollowService>();
                    var username = ctx.ArgumentValue<string>("username");
                    return $"You're {username}";
                })
                .Type<StringType>();
        }
    }
}
