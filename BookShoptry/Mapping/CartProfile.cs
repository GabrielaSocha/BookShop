using AutoMapper;
using BookShoptry.Models;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartDto>();
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductTitle, opt => opt.MapFrom(src => src.Product.Title))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price));

        CreateMap<CartItemCreateDto, CartItem>();
        CreateMap<CartItemUpdateDto, CartItem>();
    }
}
