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
            ("Body Manga Longa Nuvem", "Bodies", 69.90m, 40, true, "Body em algodão pima ultra macio, estampa de nuvens, gola envelope para facilitar a troca."),
            ("Kit Body Trio Algodão Orgânico", "Bodies", 149.90m, 25, true, "Kit com 3 bodies em algodão orgânico certificado, cores neutras unissex."),
            ("Cueiro de Malha Estampado", "Bodies", 59.90m, 35, false, "Cueiro em malha 100% algodão, estampas exclusivas do ateliê."),
            ("Manta Soft Estrelinhas", "Mantas", 129.90m, 18, true, "Manta em soft duplo, macia e quentinha, bordado personalizado disponível."),
            ("Manta Malha Tricot Baby", "Mantas", 159.90m, 12, false, "Manta em tricot fio egípcio, ponto trança, acabamento artesanal."),
            ("Saída de Maternidade Realeza", "Saída de Maternidade", 299.90m, 8, true, "Conjunto completo: body, calça, manta e touca, em tricot premium com laço de cetim."),
            ("Saída de Maternidade Girassol", "Saída de Maternidade", 279.90m, 6, false, "Conjunto amarelo suave, ideal para as primeiras fotos do bebê."),
            ("Kit Enxoval Completo 12 Peças", "Kits Enxoval", 599.90m, 5, true, "Enxoval completo para maternidade: bodies, macacões, mantas, toucas e babadores."),
            ("Kit Enxoval Essencial 6 Peças", "Kits Enxoval", 349.90m, 10, false, "Itens essenciais para os primeiros dias: bodies, fraldas de pano e touca."),
            ("Almofada de Amamentação Bordada", "Acessórios", 159.90m, 14, false, "Almofada ergonômica com capa 100% algodão e bordado com o nome do bebê."),
            ("Naninha de Coelhinho Personalizada", "Acessórios", 89.90m, 30, true, "Naninha macia em formato de coelhinho, bordado com nome, ótima para presente."),
            ("Touca e Luva Recém-Nascido", "Acessórios", 49.90m, 50, false, "Kit touca e luvas em malha canelada, protege do frio nos primeiros dias."),
            ("Body Manga Curta Bordado Ursinho", "Roupinhas", 79.90m, 30, true, "Body em algodão pima com bordado artesanal de ursinho no peito, gola de fácil abertura."),
            ("Macacão Bordado Golfinho", "Roupinhas", 99.90m, 20, false, "Macacão em algodão macio com bordado de golfinho, ideal para os passeios do bebê."),
            ("Conjunto Body e Calça Bordado Flores", "Roupinhas", 119.90m, 15, true, "Conjunto de body e calça em suedine, com bordado floral delicado feito à mão."),
            ("Almofada de Amamentação Bordada Lua e Estrelas", "Acessórios", 169.90m, 10, true, "Almofada ergonômica com capa 100% algodão e bordado de lua e estrelas, capa removível para lavagem."),
            ("Almofada de Amamentação Bordada Florzinhas", "Acessórios", 164.90m, 8, false, "Almofada de amamentação em formato de C, com bordado floral delicado feito à mão."),
            ("Toalha de Banho com Capuz Bordada Ursinho", "Toalhas", 89.90m, 25, true, "Toalha em fralda de algodão com capuz bordado de ursinho, super absorvente e macia."),
            ("Toalha Fralda Bordada Nome do Bebê", "Toalhas", 54.90m, 40, false, "Toalha fralda 100% algodão com bordado personalizado do nome do bebê."),
            ("Kit 2 Toalhas de Banho Bordadas", "Toalhas", 149.90m, 12, true, "Kit com duas toalhas de banho com capuz, bordadas com motivos infantis diferentes."),
        };

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
