using AtelieBebe.Domain.Entities;
using AtelieBebe.Domain.Exceptions;

namespace AtelieBebe.Domain.Tests.Entities;

public class AuditLogTests
{
    [Fact]
    public void Create_WithEmptyAdminId_Throws()
    {
        Assert.Throws<DomainException>(() => AuditLog.Create(Guid.Empty, "Admin", "ProductCreated", "detalhe"));
    }

    [Fact]
    public void Create_WithEmptyAction_Throws()
    {
        Assert.Throws<DomainException>(() => AuditLog.Create(Guid.NewGuid(), "Admin", " ", "detalhe"));
    }

    [Fact]
    public void Create_Valid_SetsFields()
    {
        var adminId = Guid.NewGuid();

        var log = AuditLog.Create(adminId, "Admin", "ProductCreated", "Produto X criado");

        Assert.Equal(adminId, log.AdminId);
        Assert.Equal("Admin", log.AdminName);
        Assert.Equal("ProductCreated", log.Action);
        Assert.Equal("Produto X criado", log.Details);
    }
}
