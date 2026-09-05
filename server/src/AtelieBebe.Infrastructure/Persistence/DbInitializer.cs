using AtelieBebe.Application.Common;
using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.ValueObjects;
using AtelieBebe.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieBebe.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<Application.Abstractions.IPasswordHasher>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await dbContext.Database.MigrateAsync();

        await SeedAdminAsync(dbContext, passwordHasher, configuration);
        await SeedProductsAsync(dbContext);
    }

    private static async Task SeedAdminAsync(AppDbContext dbContext, Application.Abstractions.IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        var email = configuration["AdminSeed:Email"] ?? "admin@ateliebebe.com.br";
        var password = configuration["AdminSeed:Password"] ?? "admin123";

        var normalizedEmail = Email.Create(email);
        var exists = await dbContext.Admins.AnyAsync(a => a.Email == normalizedEmail);
        if (exists) return;

        var admin = Admin.Create("Administradora do Ateliê", normalizedEmail, passwordHasher.Hash(password));
        dbContext.Admins.Add(admin);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(AppDbContext dbContext)
    {
        var seedData = new (string Name, string Category, decimal Price, int Stock, bool Featured, string Description)[]
        {
            // Kit Ombro e Boca
            ("Kit Fralda de Ombro e Boca Ursinho Bordado", "Kit Ombro e Boca", 79.90m, 30, true, "Kit com 1 fralda de ombro e 1 fralda de boca em algodão, bordado artesanal de ursinho."),
            ("Kit Fralda de Ombro e Boca Nuvem Bordada", "Kit Ombro e Boca", 79.90m, 28, true, "Kit com 1 fralda de ombro e 1 fralda de boca, bordado de nuvens, tecido macio e absorvente."),
            ("Kit 3 Fraldas de Ombro e Boca Coordenadas", "Kit Ombro e Boca", 119.90m, 18, true, "Kit com 3 conjuntos de fralda de ombro e boca coordenados, estampas exclusivas do ateliê."),
            // Fralda de Ombro (só ombro)
            ("Fralda de Ombro Bordada Golfinho", "Fralda de Ombro", 49.90m, 35, false, "Fralda de ombro avulsa em algodão, bordado de golfinho, tamanho reforçado para proteger a roupa."),
            ("Fralda de Ombro Xadrez Piquet", "Fralda de Ombro", 44.90m, 40, false, "Fralda de ombro em piquet xadrez, absorvente e resistente ao uso diário."),
            ("Fralda de Ombro Bordada com Nome", "Fralda de Ombro", 54.90m, 22, true, "Fralda de ombro avulsa com bordado personalizado do nome do bebê."),
            // Fralda de Boca (só boca)
            ("Fralda de Boca Tricô Bordada", "Fralda de Boca", 29.90m, 45, false, "Fralda de boca avulsa em tricô, bordado delicado, ideal para arrotinhos e babados."),
            ("Kit 3 Fraldas de Boca Estampadas", "Fralda de Boca", 39.90m, 32, true, "Kit com 3 fraldas de boca em algodão estampado, super macias e absorventes."),
            ("Fralda de Boca Bordada Florzinha", "Fralda de Boca", 34.90m, 27, false, "Fralda de boca avulsa com bordado floral feito à mão."),
        };

        // The ateliê now specializes exclusively in shoulder/mouth burp cloths — remove any product
        // left over from the previous general-purpose catalog (or restored from an old backup) so the
        // store never shows a category outside this specialization.
        var allowedCategories = seedData.Select(p => p.Category).ToHashSet();
        var discontinued = await dbContext.Products.Where(p => !allowedCategories.Contains(p.Category)).ToListAsync();
        if (discontinued.Count > 0)
        {
            dbContext.Products.RemoveRange(discontinued);
            await dbContext.SaveChangesAsync();
        }

        // Seeded per-item (not gated on an empty table) so re-running after the catalog already has
        // products still fills in any new seed entries — e.g. a fresh deploy that inherited an older
        // database — without duplicating ones that were seeded before.
        var existingSlugs = (await dbContext.Products.Select(p => p.Slug).ToListAsync()).ToHashSet();

        var newProducts = seedData
            .Select((p, index) => (Product: p, ImageIndex: index + 1))
            .Where(x => !existingSlugs.Contains(SlugHelper.Slugify(x.Product.Name)))
            .Select(x => Product.Create(
                x.Product.Name,
                SlugHelper.Slugify(x.Product.Name),
                x.Product.Description,
                Money.FromReais(x.Product.Price),
                x.Product.Category,
                $"https://picsum.photos/seed/atelie-bebe-{x.ImageIndex}/600/600",
                x.Product.Stock,
                x.Product.Featured))
            .ToList();

        if (newProducts.Count == 0) return;

        dbContext.Products.AddRange(newProducts);
        await dbContext.SaveChangesAsync();
    }
}
