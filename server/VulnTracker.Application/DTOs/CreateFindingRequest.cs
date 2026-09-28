using System.ComponentModel.DataAnnotations;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Application.DTOs;

public class CreateFindingRequest
{
    [Required, StringLength(200)]
    public string Title { get; init; } = "";

    [Required]
    public string Description { get; init; } = "";

    [Required, StringLength(200)]
    public string AffectedAsset { get; init; } = "";

    [Required, EnumDataType(typeof(Severity))]
    public Severity Severity { get; init; }

    [Range(0.0, 10.0)]
    public decimal? CvssScore { get; init; }

    public DateTimeOffset? DiscoveryDate { get; init; }
}