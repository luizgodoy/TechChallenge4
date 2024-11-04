using AutoMapper;
using TechChallenge.Contract.Contact;
using TechChallenge.Core.DTO;
using TechChallenge.Domain.Models;

namespace TechChallenge.API.AutoMapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Contact, ContactDto>().ReverseMap();
            CreateMap<AddContactMessage, ContactDto>().ReverseMap();
            CreateMap<EditContactMessage, ContactDto>().ReverseMap();

            CreateMap<State, StateDto>().ReverseMap();
        }
    }
}
