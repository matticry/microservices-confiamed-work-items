using AutoMapper;
using ItemsTrabajo.Application.DTOs.WorkItem;
using ItemsTrabajo.Domain.Entities;
using ItemsTrabajo.Domain.Enums;

namespace ItemsTrabajo.Application.Mapping;

public class WorkItemProfile : Profile
{
    public WorkItemProfile()
    {
        // Entity → DTO
        CreateMap<WorkItem, WorkItemDto>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.IdWi))
            .ForMember(dest => dest.Code,
                opt => opt.MapFrom(src => src.CodeWi))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.DescriptionWi))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => MapStatus(src.StatusWi)))
            .ForMember(dest => dest.Relevance,
                opt => opt.MapFrom(src => MapRelevance(src.Relevance)))
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.MinValue))
            .ForMember(dest => dest.ExpirationDate,
                opt => opt.MapFrom(src => src.ExpirationDate ?? DateTime.MinValue));

        // CreateDto → Entity
        CreateMap<CreateWorkItemDto, WorkItem>()
            .ForMember(dest => dest.CodeWi,
                opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.DescriptionWi,
                opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Relevance,
                opt => opt.MapFrom(src => src.Relevance))
            .ForMember(dest => dest.ExpirationDate,
                opt => opt.MapFrom(src => src.ExpirationDate))
            // Ignorar campos que se asignan en el Handler
            .ForMember(dest => dest.IdWi, opt => opt.Ignore())
            .ForMember(dest => dest.StatusWi, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserWorks, opt => opt.Ignore());
    }

    private static string MapStatus(string? status) => status switch
    {
        "0" => nameof(WorkItemStatus.Pending),
        "1" => nameof(WorkItemStatus.Assigned),
        "2" => nameof(WorkItemStatus.Completed),
        _ => "Unknown"
    };

    private static string MapRelevance(string? relevance) => relevance switch
    {
        "H" => nameof(Relevance.High),
        "L" => nameof(Relevance.Low),
        _ => "Unknown"
    };
}