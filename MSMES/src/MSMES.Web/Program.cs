using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MSMES.Application.Auth;
using MSMES.Application.Dashboard;
using MSMES.Application.Equipment;
using MSMES.Application.Inventory;
using MSMES.Application.LotManagement;
using MSMES.Application.Process;
using MSMES.Application.PurchaseOrder;
using MSMES.Application.Quality;
using MSMES.Application.SalesOrder;
using MSMES.Application.Shipment;
using MSMES.Application.WorkOrder;
using MSMES.Infrastructure;
using MSMES.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (repositories, JWT, hasher)
builder.Services.AddMsmesInfrastructure(builder.Configuration);

// Application handlers
builder.Services.AddScoped<CreateSalesOrderHandler>();
builder.Services.AddScoped<GetSalesOrderHandler>();
builder.Services.AddScoped<ListSalesOrdersHandler>();
builder.Services.AddScoped<CreatePurchaseOrderHandler>();
builder.Services.AddScoped<CreateWorkOrderHandler>();
builder.Services.AddScoped<UpdateWorkOrderStatusHandler>();
builder.Services.AddScoped<CreateLotHandler>();
builder.Services.AddScoped<GetLotHistoryHandler>();
builder.Services.AddScoped<UpdateLotStatusHandler>();
builder.Services.AddScoped<ListAllLotsHandler>();
builder.Services.AddScoped<CreateShipmentHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<GetInventoryStatusHandler>();
builder.Services.AddScoped<CreateInventoryTransactionHandler>();
builder.Services.AddScoped<CreateQualityInspectionHandler>();
builder.Services.AddScoped<GetQualityReportHandler>();
builder.Services.AddScoped<GetEquipmentStatusHandler>();
builder.Services.AddScoped<CreateMaintenanceHandler>();
builder.Services.AddScoped<CreateProductionResultHandler>();
builder.Services.AddScoped<GetProductionResultHandler>();
builder.Services.AddScoped<DashboardHandler>();

// JWT auth
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MSMES API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Bearer 토큰을 입력하세요."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
