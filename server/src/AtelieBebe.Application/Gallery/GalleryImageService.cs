using AtelieBebe.Application.Abstractions;
using AtelieBebe.Application.Exceptions;
using AtelieBebe.Domain.Entities;

namespace AtelieBebe.Application.Gallery;

public sealed class GalleryImageService : IGalleryImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;

    public GalleryImageService(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<GalleryImageDto>> ListAsync(CancellationToken ct = default)
    {
        var images = await _unitOfWork.GalleryImages.ListAsync(ct);
        return images.Select(ToDto).ToList();
    }

    public async Task<GalleryImageDto> AddAsync(string url, CancellationToken ct = default)
    {
        var image = GalleryImage.Create(url);
        _unitOfWork.GalleryImages.Add(image);
        await _unitOfWork.SaveChangesAsync(ct);
        return ToDto(image);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var image = await _unitOfWork.GalleryImages.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Imagem da galeria", id);

        _unitOfWork.GalleryImages.Remove(image);
        await _unitOfWork.SaveChangesAsync(ct);
        await _fileStorage.DeleteAsync(image.Url, ct);
    }

    private static GalleryImageDto ToDto(GalleryImage g) => new(g.Id, g.Url, g.CreatedAt);
}
