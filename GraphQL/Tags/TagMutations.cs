using DataCommon.Models.Documents;
using DataCommon.Models.Utils;
using DataService.DTO;
using DataService.Services.Interfaces.Documents;
using GraphQL.TypesUtils;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GraphQL.Tags
{
    public class TagMutations
    {
        public async Task<TagDTO> AddTag([Service] ITagService tagService,TagDTO input)
        {
            var result = await tagService.Add(input);
            return result;
        }

        public async Task<TagDTO> UpdateTag([Service] ITagService tagService,string id, TagDTO input)
        {
            input.Id = id;
            var result = await tagService.Update(id,input);
            return result;
        }

        public async Task<ResultBool> DeleteTagById([Service] ITagService tagService, string id)
        {
            var tag = await tagService.GetById(id);
            var result = await tagService.Remove(tag);
            return new ResultBool(result, DateTime.Now); ;
        }

        public async Task<ResultBool> DeleteTagByName([Service] ITagService tagService, string name)
        {
            var result = await tagService.DeleteTagByName(name);
            return new ResultBool(result, DateTime.Now); ;
        }

        public async Task<ResultBool> IgnoreTag([Service] ITagService tagService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, string tag)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            Console.WriteLine(userDb.Id);
            var result = await tagService.IgnoreTagForUser(tag, userDb.Id.ToString());
            return new ResultBool(result, DateTime.Now); ;

        }

        public async Task<ResultBool> UnignoreTag([Service] ITagService tagService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, string tag)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var result = await tagService.UnignoreTagForUser(tag, userDb.Id.ToString());
            return new ResultBool(result,DateTime.Now);
        }
    }
    public class TagMutationsExtension : ObjectTypeExtension<TagMutations>
    {
        protected override void Configure(IObjectTypeDescriptor<TagMutations> descriptor)
        {

            descriptor.Name(OperationTypeNames.Mutation);

            descriptor.Field(t => t.AddTag(default!, default!))
                .Authorize(new[] { UserRoles.Admin })
                .Description("Create New Tag");

            descriptor.Field(t => t.UpdateTag(default!, default!, default!))
                .Authorize(new[] { UserRoles.Admin })
                .Description("Update Existing Tag By Name");

            descriptor.Field(t => t.DeleteTagById(default!, default!))
                .Description("Remove Tag By Id")
                .Authorize(new[] {UserRoles.Admin} )
                .Name("removeTag");

            descriptor.Field(t => t.DeleteTagByName(default!, default!))
                .Description("Remove Tag By Name")
                .Authorize(new[] { UserRoles.Admin })
                .Name("removeTagByName");

            descriptor.Field(t => t.IgnoreTag(default!, default!, default!, default!))
                .Authorize()
                .Description("Add tag to ignore list for user");

            descriptor.Field(t => t.UnignoreTag(default!, default!, default!, default!))
                .Authorize()
                .Description("Remove tag from ignore list for user");


        }
    }

}
