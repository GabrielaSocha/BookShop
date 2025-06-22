using AutoMapper;
using BookShoptry.Models;
using BookShoptry.Dtos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookShoptry.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<ProductCreateDto, Product>();
        }
    }
}
