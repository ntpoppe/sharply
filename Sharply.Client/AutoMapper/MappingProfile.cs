using AutoMapper;
using Sharply.Client.ViewModels;
using Sharply.Shared.Models;

namespace Sharply.Client.AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ServerDto, ServerViewModel>();
        CreateMap<ChannelDto, ChannelViewModel>();
        CreateMap<MessageDto, MessageViewModel>();
    }
}

