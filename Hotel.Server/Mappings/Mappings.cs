using AutoMapper;
using Hotel.Server.Helpers;
using Hotel.Server.Hotels;
using Hotel.Server.Users;

namespace Hotel.Server.Mappings;

public class Mappings : Profile
{
    public Mappings()
    {
        CreateMap<User, UserResponse>();
        CreateMap<UserAddRequest, User>().ForMember(e => e.Password, opt => opt.Ignore());
        CreateMap<UserUpdateRequest, User>().ForMember(e => e.Password, opt => opt.Ignore());
        CreateMap<UserRegistrationRequest, User>().ForMember(e => e.Password, opt => opt.Ignore());

        CreateMap<Hotel.Server.Hotels.Hotel, HotelResponse>();
        CreateMap<HotelAddRequest, Hotel.Server.Hotels.Hotel>();
        CreateMap<HotelUpdateRequest, Hotel.Server.Hotels.Hotel>();
    }
}
