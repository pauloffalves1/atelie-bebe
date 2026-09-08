using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Common;
using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Application.Audit;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task RecordAsync(Guid adminId, string adminName, string action, string details, CancellationToken ct = default)
    {
        _unitOfWork.AuditLogs.Add(AuditLog.Create(adminId, adminName, action, details));
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<AuditLogDto>> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var (items, totalItems) = await _unitOfWork.AuditLogs.ListAsync(normalizedPage, normalizedPageSize, ct);

        return new PagedResult<AuditLogDto>(
            items.Select(a => new AuditLogDto(a.Id, a.AdminName, a.Action, a.Details, a.CreatedAt)).ToList(),
            normalizedPage, normalizedPageSize, totalItems);
    }
}
