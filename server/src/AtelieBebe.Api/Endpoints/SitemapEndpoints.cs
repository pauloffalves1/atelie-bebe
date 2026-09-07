using System.Text;
using AtelieBebe.Application.Products;
using AtelieBebe.Infrastructure;
using Microsoft.Extensions.Options;

namespace AtelieBebe.Api.Endpoints;

public static class SitemapEndpoints
{
    private static readonly string[] StaticPaths =
    [
        "/",
        "/loja",
        "/sobre",
        "/galeria",
        "/contato",
    ];

    /// <summary>
    /// Generated at request time (not a static file) so it always reflects the current catalog —
    /// robots.txt points crawlers at /api/sitemap.xml, reusing the /api/* proxy rule Nginx already
    /// has in production instead of needing a dedicated route at the SPA's own root.
    /// </summary>
    public static void MapSitemapEndpoints(this WebApplication app)
    {
        app.MapGet("/api/sitemap.xml", async (IProductService productService, IOptions<AppUrlOptions> appUrls, CancellationToken ct) =>
        {
            var siteUrl = appUrls.Value.PublicUrl.TrimEnd('/');
            var products = await productService.ListAsync(category: null, onlyActive: true, page: 1, pageSize: 10_000, customerId: null, ct: ct);

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            foreach (var path in StaticPaths)
                AppendUrl(sb, $"{siteUrl}{path}", "weekly");

            foreach (var product in products.Items)
                AppendUrl(sb, $"{siteUrl}/produto/{product.Slug}", "monthly");

            sb.AppendLine("</urlset>");

            return Results.Text(sb.ToString(), "application/xml");
        }).WithTags("Sitemap");
    }

    private static void AppendUrl(StringBuilder sb, string loc, string changeFreq)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{System.Security.SecurityElement.Escape(loc)}</loc>");
        sb.AppendLine($"    <changefreq>{changeFreq}</changefreq>");
        sb.AppendLine("  </url>");
    }
}
