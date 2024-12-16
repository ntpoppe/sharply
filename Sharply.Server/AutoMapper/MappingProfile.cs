using AutoMapper;
using Sharply.Server.Models;
using Sharply.Shared.Models;

namespace Sharply.Server.Automapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map Server -> ServerDto
        CreateMap<Models.Server, ServerDto>()
            .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => src.Channels)); // Maps Channels

        // Map Channel -> ChannelDto
        CreateMap<Channel, ChannelDto>()
            .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages)); // Maps Messages

        // Map Message -> MessageDto
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username)); // Maps Username to User.Username
    }
}

