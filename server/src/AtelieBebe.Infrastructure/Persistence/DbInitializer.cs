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
        if (await dbContext.Products.AnyAsync()) return;

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
        };

        var products = seedData.Select((p, index) => Product.Create(
            p.Name,
            SlugHelper.Slugify(p.Name),
            p.Description,
            Money.FromReais(p.Price),
            p.Category,
            $"https://picsum.photos/seed/atelie-bebe-{index + 1}/600/600",
            p.Stock,
            p.Featured));

        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync();
    }
}
