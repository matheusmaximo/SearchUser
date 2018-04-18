﻿using AutoMapper;
using SearchUser.Entities.Models;
using SearchUser.Entities.ViewModel;

namespace SearchUser.Api.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Telephone, TelephoneViewModel>()
                .ReverseMap();

            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(e => e.Password, e => e.Ignore())
                .ReverseMap()
                .ForMember(e => e.UserName, e => e.MapFrom(c => c.Email));

            CreateMap<ApplicationUser, SignedInUserViewModel>();
        }
    }
}
