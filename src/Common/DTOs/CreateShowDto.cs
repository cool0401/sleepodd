namespace Podcast.Common.DTOs;

public record CreateShowDto(
    string Title,
    string Description,
    string Image,
    string Category
);