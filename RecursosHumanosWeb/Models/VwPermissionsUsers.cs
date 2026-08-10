using System;

namespace RecursosHumanosWeb.Models;

public partial class VwPermissionsUsers
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int? RequestedDays { get; set; }

    public bool WithPay { get; set; }

    public TimeOnly? EntryTime { get; set; }

    public TimeOnly? TimeToCover { get; set; }

    public string? Evidence { get; set; }

    public DateOnly? FromDay { get; set; }

    public DateOnly? ToDay { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? AuthorizedAt { get; set; }

    public int PermissionTypeId { get; set; }

    public int CutoffId { get; set; }

    public bool Status { get; set; }

    public int RequestingUserId { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public int? AuthorizingUserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int AreaId { get; set; }
}
