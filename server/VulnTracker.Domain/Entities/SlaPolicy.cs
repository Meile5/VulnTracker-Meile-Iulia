using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VulnTracker.Domain.Enums;

namespace VulnTracker.Domain.Entities;

public class SlaPolicy
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Severity Severity { get; set; }
    public int Days { get; set; }
}
