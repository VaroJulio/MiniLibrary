using AutoMapper;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Application.Books.Mappings;

/// <summary>
/// AutoMapper profile for Book entity to DTO mappings.
/// </summary>
public class BookMappingProfile : Profile
{
    public BookMappingProfile()
    {
        CreateMap<Book, BookResponse>()
            .ForMember(dest => dest.Isbn, opt => opt.MapFrom(src => src.ISBN))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
