using AutoMapper;
using DataCommon.Models.Documents;
using DataService.DTO;
using DataService.Utils;

namespace GraphQL.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PageWrapper<Tag>, PageWrapper<TagDTO>>().ReverseMap(); ;
            CreateMap<Tag, TagDTO>().ReverseMap();

            CreateMap<PageWrapper<SharePost>, PageWrapper<SharePostDTO>>().ReverseMap(); ;
            CreateMap<SharePost, SharePostDTO>().ReverseMap();

            CreateMap<PageWrapper<DataCommon.Models.Documents.Likes>, PageWrapper<LikesDTO>>().ReverseMap(); ;
            CreateMap<DataCommon.Models.Documents.Likes, LikesDTO>().ReverseMap();

            CreateMap<PageWrapper<Post>, PageWrapper<PostDTO>>().ReverseMap();
            CreateMap<PostMultimedia, PostMultimediaDTO>().ReverseMap();
            CreateMap<Post, PostDTO>().ForMember(pDTO => pDTO.MultimediaDTO, o => o.MapFrom(p => p.Multimedia));
            CreateMap<PostDTO, Post>().ForMember(p => p.Multimedia, o => o.MapFrom(pDTO => pDTO.MultimediaDTO));

            CreateMap<ApplicationUser, UserDTO>();

            CreateMap<UserDTO, ApplicationUser>();

            CreateMap<Follower, FollowerDTO>().ReverseMap();


        }
    }
}
