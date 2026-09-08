using System.Text.Json;
using AtelieBebe.Application.Abstractions;
using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Application.Cart;

public sealed class CartSyncService : ICartSyncService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartSyncService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task SaveAsync(Guid customerId, CartSyncRequest request, CancellationToken ct = default)
    {
        var existing = await _unitOfWork.CartSnapshots.GetByCustomerAsync(customerId, ct);

        if (request.Items.Count == 0)
        {
            if (existing is not null)
            {
                _unitOfWork.CartSnapshots.Remove(existing);
                await _unitOfWork.SaveChangesAsync(ct);
            }
            return;
        }

        var itemsJson = JsonSerializer.Serialize(request.Items);

        if (existing is null)
            _unitOfWork.CartSnapshots.Add(CartSnapshot.Create(customerId, itemsJson));
        else
            existing.ReplaceItems(itemsJson);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
