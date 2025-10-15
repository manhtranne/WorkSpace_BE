// src/WorkSpace.Application/DTOs/WorkSpaces/WorkSpaceFilter.cs
namespace WorkSpace.Application.DTOs.WorkSpaces;

public record WorkSpaceFilter(
    int? WorkSpaceRoomTypeId, // Changed from WorkSpaceTypeId
    string? City,
    decimal? MinPricePerDay,
    decimal? MaxPricePerDay,
    int? MinCapacity,
    bool? OnlyVerified,
    bool? OnlyActived,
    DateTimeOffset? DesiredStartUtc,
    DateTimeOffset? DesiredEndUtc);