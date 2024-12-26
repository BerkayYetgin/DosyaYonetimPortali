using WebSitem.Models;
using AutoMapper;
using WebSitem.ViewModel;

namespace WebSitem.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
           
            CreateMap<AppUser,LoginViewModel>().ReverseMap();
            CreateMap<AppUser,RegisterViewModel>().ReverseMap();

            // Survey mappings
           
        }
    }
}
