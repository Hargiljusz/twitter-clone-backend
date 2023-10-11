using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Services.Interfaces.Documents;
using DataService.Utils;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;

namespace GraphQL.Tags
{
    public class TagQueries
    {
        public async Task<TagDTO> GetTagByName([Service] ITagService tagService,string name)
        {
            var result = await tagService.GetTagByName(name);
            return result;
        }

        public async Task<TagDTO> GetTagById([Service] ITagService tagService, string id)
        {
            var result = await tagService.GetById(id);
            return result;
        }

        public async Task<PageWrapper<TagDTO>> GetTags([Service] ITagService tagService, int page = 0,int size = 10)
        {
            var result = await tagService.GetAllPageable(pageSize: size, pageNumber: page);
            return result;
        }

        public async Task<PageWrapper<TagDTO>> Search([Service] ITagService tagService, string q,int page = 0, int size = 10)
        {
            var result = await tagService.SearchPageable(q, size, page);
            return result;
        }

        public async Task<PageWrapper<TagDTO>> GetPopularTags([Service] ITagService tagService, int page = 0, int size = 10)
        {
            var result = await tagService.GetPopularTagsInThis(TimeDuration.WEEK ,size, page);
            return result;
        }

        public async Task<PageWrapper<TagDTO>> GetMyIgnoreTags([Service] ITagService tagService, [Service] UserManager<ApplicationUser> userManager, ClaimsPrincipal user, int page = 0, int size = 10)
        {
            var userEmail = user.Claims.First(c => c.Type == ClaimTypes.Email).Value;
            var userDb = await userManager.FindByEmailAsync(userEmail);
            var sort = Builders<DataCommon.Models.Documents.Tag>.Sort.Ascending(t => t.Name);
            var result = await tagService.GetAllIgnoredTagsForUserPageableAndSort(userDb.Id.ToString(), sort, size, page);
            return result;
        }
    }

    public class TagQueriesExtension : ObjectTypeExtension<TagQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<TagQueries> descriptor)
        {

            descriptor.Name(OperationTypeNames.Query);

            descriptor.Field(t => t.GetTagByName(default!, default!))
            .Description("This query return tag by name");

            descriptor.Field(t => t.GetTagById(default!, default!))
            .Description("This query return tag by id");


            descriptor.Field(t => t.GetTags(default!, default!, default!))
            .Description("This query return tags");

            descriptor.Field(t => t.Search(default!, default!, default!, default!))
                .Description("Search tags By Name");

            descriptor.Field(t => t.GetPopularTags( default!, default!, default!))
               .Description("Poplar tags In This Week");

            descriptor.Field(t => t.GetMyIgnoreTags(default!, default!, default!, default!, default!))
               .Authorize()
               .Description("Get my ignored tags");
        }
    }


    
}
