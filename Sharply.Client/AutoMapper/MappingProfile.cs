using AutoMapper;
using Sharply.Shared.Models;
using Sharply.Client.ViewModels;

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

