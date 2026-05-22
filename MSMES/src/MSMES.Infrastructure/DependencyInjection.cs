using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MSMES.Application.Common;
using MSMES.Application.Dashboard;
using MSMES.Domain.Alert;
using MSMES.Domain.Bom;
using MSMES.Domain.Partner;
using MSMES.Domain.Common;
using MSMES.Domain.Equipment;
using MSMES.Domain.Inventory;
using MSMES.Domain.LotManagement;
using MSMES.Domain.Process;
using MSMES.Domain.ProductionPlan;
using MSMES.Domain.PurchaseOrder;
using MSMES.Domain.Quality;
using MSMES.Domain.Receiving;
using MSMES.Domain.Settings;
using MSMES.Domain.Spc;
using MSMES.Domain.SalesOrder;
using MSMES.Domain.Shipment;
using MSMES.Domain.WorkOrder;
using MSMES.Infrastructure.Auth;
using MSMES.Infrastructure.Persistence;
using MSMES.Infrastructure.Repositories;
using MSMES.Infrastructure.Settings;

namespace MSMES.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMsmesInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddScoped<ISalesOrderRepository, SqlSalesOrderRepository>();
        services.AddScoped<IPurchaseOrderRepository, SqlPurchaseOrderRepository>();
        services.AddScoped<IWorkOrderRepository, SqlWorkOrderRepository>();
        services.AddScoped<ILotRepository, SqlLotRepository>();
        services.AddScoped<IShipmentRepository, SqlShipmentRepository>();
        services.AddScoped<IUserRepository, SqlUserRepository>();
        services.AddScoped<ICommonCodeRepository, SqlCommonCodeRepository>();
        services.AddScoped<IInventoryRepository, SqlInventoryRepository>();
        services.AddScoped<IQualityRepository, SqlQualityRepository>();
        services.AddScoped<IEquipmentRepository, SqlEquipmentRepository>();
        services.AddScoped<IProcessRepository, SqlProcessRepository>();
        services.AddScoped<IDashboardRepository, SqlDashboardRepository>();
        services.AddScoped<IAuditLogRepository, SqlAuditLogRepository>();
        services.AddScoped<IBomRepository, SqlBomRepository>();
        services.AddScoped<IProductionPlanRepository, SqlProductionPlanRepository>();
        services.AddScoped<IAlertRepository, SqlAlertRepository>();
        services.AddScoped<IGoodsReceiptRepository, SqlGoodsReceiptRepository>();
        services.AddScoped<ISpcRepository, SqlSpcRepository>();
        services.AddScoped<IPartnerRepository, SqlPartnerRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        // Application.Common 인터페이스로 등록 (Application 레이어에서 주입 가능)
        // JwtService/BCryptPasswordHasher는 Infrastructure.Auth alias 인터페이스를 구현하며,
        // 이 alias 인터페이스는 Application.Common 인터페이스를 상속하므로
        // Application.Common.IJwtService / IPasswordHasher 로도 resolve 됩니다.
        services.AddSingleton<Application.Common.IJwtService, JwtService>();
        services.AddSingleton<Application.Common.IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
