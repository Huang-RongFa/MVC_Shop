using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWeb.Domain.Entities.Accounts;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Inventory;
using MyWeb.Domain.Entities.Logs;
using MyWeb.Domain.Entities.Orders;
using MyWeb.Domain.Entities.Payments;
using MyWeb.Domain.Entities.Promotions;

namespace MyWeb.Data.Configurations;

public static class EcommerceModelConfigurationExtensions
{
    public static void ApplyEcommerceConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAccounts();
        modelBuilder.ConfigureCatalog();
        modelBuilder.ConfigureInventory();
        modelBuilder.ConfigureOrders();
        modelBuilder.ConfigurePayments();
        modelBuilder.ConfigurePromotions();
        modelBuilder.ConfigureLogs();
    }

    private static void ConfigureAccounts(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", table =>
            {
                table.HasCheckConstraint("CK_Users_UserType", "UserType IN ('Customer','Admin','Staff')");
                table.HasCheckConstraint("CK_Users_Status", "Status IN ('Active','Inactive','Locked')");
            });
            entity.HasKey(x => x.UserId).HasName("PK_Users");
            entity.Property(x => x.UserNo).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Account).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.PasswordSalt).HasMaxLength(200);
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UserType).HasMaxLength(30).HasDefaultValue("Customer").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(x => x.UserNo).IsUnique().HasDatabaseName("UK_Users_UserNo");
            entity.HasIndex(x => x.Account).IsUnique().HasDatabaseName("UK_Users_Account");
            entity.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UK_Users_Email").HasFilter(null);
            entity.HasIndex(x => x.Account).HasDatabaseName("IX_Users_Account");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.RoleId).HasName("PK_Roles");
            entity.Property(x => x.RoleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RoleName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsSystemRole).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(x => x.RoleCode).IsUnique().HasDatabaseName("UK_Roles_RoleCode");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(x => x.PermissionId).HasName("PK_Permissions");
            entity.Property(x => x.PermissionCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PermissionName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ModuleName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.PermissionCode).IsUnique().HasDatabaseName("UK_Permissions_PermissionCode");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(x => x.UserRoleId).HasName("PK_UserRoles");
            entity.Property(x => x.AssignedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique().HasDatabaseName("UK_UserRoles_UserId_RoleId");
            entity.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_UserRoles_Users");
            entity.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_UserRoles_Roles");
            entity.HasOne(x => x.AssignedByUser).WithMany(x => x.AssignedUserRoles).HasForeignKey(x => x.AssignedBy).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_UserRoles_AssignedBy");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(x => x.RolePermissionId).HasName("PK_RolePermissions");
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique().HasDatabaseName("UK_RolePermissions_RoleId_PermissionId");
            entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_RolePermissions_Roles");
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_RolePermissions_Permissions");
        });

        modelBuilder.Entity<UserLoginLog>(entity =>
        {
            entity.ToTable("UserLoginLogs", table => table.HasCheckConstraint("CK_UserLoginLogs_LoginResult", "LoginResult IN ('Success','Failed','Logout')"));
            entity.HasKey(x => x.LoginLogId).HasName("PK_UserLoginLogs");
            entity.Property(x => x.Account).HasMaxLength(100);
            entity.Property(x => x.LoginResult).HasMaxLength(30).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(300);
            entity.Property(x => x.IpAddress).HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.LoginAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.User).WithMany(x => x.LoginLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_UserLoginLogs_Users");
        });
    }

    private static void ConfigureCatalog(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasKey(x => x.CategoryId).HasName("PK_ProductCategories");
            entity.Property(x => x.CategoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(x => x.CategoryCode).IsUnique().HasDatabaseName("UK_ProductCategories_CategoryCode");
            entity.HasOne(x => x.ParentCategory).WithMany(x => x.ChildCategories).HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ProductCategories_Parent");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", table => table.HasCheckConstraint("CK_Products_Status", "Status IN ('Draft','Active','Inactive','Archived')"));
            entity.HasKey(x => x.ProductId).HasName("PK_Products");
            entity.Property(x => x.ProductNo).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.BrandName).HasMaxLength(100);
            entity.Property(x => x.ShortDescription).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.IsFeatured).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(x => x.ProductNo).IsUnique().HasDatabaseName("UK_Products_ProductNo");
            entity.HasIndex(x => x.ProductName).HasDatabaseName("IX_Products_ProductName");
            entity.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Products_ProductCategories");
        });

        modelBuilder.Entity<ProductSku>(entity =>
        {
            entity.ToTable("ProductSkus", table =>
            {
                table.HasCheckConstraint("CK_ProductSkus_ListPrice", "ListPrice >= 0");
                table.HasCheckConstraint("CK_ProductSkus_SalePrice", "SalePrice >= 0");
                table.HasCheckConstraint("CK_ProductSkus_CostPrice", "CostPrice IS NULL OR CostPrice >= 0");
                table.HasCheckConstraint("CK_ProductSkus_Status", "Status IN ('Active','Inactive')");
            });
            entity.HasKey(x => x.SkuId).HasName("PK_ProductSkus");
            entity.Property(x => x.SkuNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Barcode).HasMaxLength(100);
            entity.Property(x => x.SkuName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SpecText).HasMaxLength(500);
            entity.Property(x => x.ListPrice).HasPrecision(18, 2);
            entity.Property(x => x.SalePrice).HasPrecision(18, 2);
            entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            entity.Property(x => x.Weight).HasPrecision(18, 3);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(x => x.ProductId).HasDatabaseName("IX_ProductSkus_ProductId");
            entity.HasIndex(x => x.SkuNo).IsUnique().HasDatabaseName("UK_ProductSkus_SkuNo");
            entity.HasIndex(x => x.Barcode).IsUnique().HasDatabaseName("UK_ProductSkus_Barcode").HasFilter(null);
            entity.HasOne(x => x.Product).WithMany(x => x.Skus).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ProductSkus_Products");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasKey(x => x.ImageId).HasName("PK_ProductImages");
            entity.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.AltText).HasMaxLength(200);
            entity.Property(x => x.IsMainImage).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ProductImages_Products");
            entity.HasOne(x => x.Sku).WithMany(x => x.Images).HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ProductImages_ProductSkus");
        });

        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.ToTable("ProductAttributes", table => table.HasCheckConstraint("CK_ProductAttributes_InputType", "InputType IN ('Text','Select','Number')"));
            entity.HasKey(x => x.AttributeId).HasName("PK_ProductAttributes");
            entity.Property(x => x.AttributeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AttributeName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.InputType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.AttributeCode).IsUnique().HasDatabaseName("UK_ProductAttributes_AttributeCode");
        });

        modelBuilder.Entity<ProductAttributeValue>(entity =>
        {
            entity.ToTable("ProductAttributeValues");
            entity.HasKey(x => x.AttributeValueId).HasName("PK_ProductAttributeValues");
            entity.Property(x => x.AttributeValue).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => new { x.SkuId, x.AttributeId }).IsUnique().HasDatabaseName("UK_ProductAttributeValues_SkuId_AttributeId");
            entity.HasOne(x => x.Sku).WithMany(x => x.AttributeValues).HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ProductAttributeValues_ProductSkus");
            entity.HasOne(x => x.Attribute).WithMany(x => x.Values).HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ProductAttributeValues_ProductAttributes");
        });
    }

    private static void ConfigureInventory(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            entity.HasKey(x => x.WarehouseId).HasName("PK_Warehouses");
            entity.Property(x => x.WarehouseCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WarehouseName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.ContactName).HasMaxLength(100);
            entity.Property(x => x.ContactPhone).HasMaxLength(30);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.WarehouseCode).IsUnique().HasDatabaseName("UK_Warehouses_WarehouseCode");
        });

        modelBuilder.Entity<InventoryStock>(entity =>
        {
            entity.ToTable("InventoryStocks", table =>
            {
                table.HasCheckConstraint("CK_InventoryStocks_OnHandQty", "OnHandQty >= 0");
                table.HasCheckConstraint("CK_InventoryStocks_ReservedQty", "ReservedQty >= 0");
                table.HasCheckConstraint("CK_InventoryStocks_SafetyStockQty", "SafetyStockQty >= 0");
                table.HasCheckConstraint("CK_InventoryStocks_AvailableLogic", "OnHandQty >= ReservedQty");
            });
            entity.HasKey(x => x.InventoryStockId).HasName("PK_InventoryStocks");
            entity.Property(x => x.OnHandQty).HasDefaultValue(0);
            entity.Property(x => x.ReservedQty).HasDefaultValue(0);
            entity.Property(x => x.AvailableQty).HasComputedColumnSql("OnHandQty - ReservedQty");
            entity.Property(x => x.SafetyStockQty).HasDefaultValue(0);
            entity.HasIndex(x => new { x.WarehouseId, x.SkuId }).IsUnique().HasDatabaseName("UK_InventoryStocks_WarehouseId_SkuId");
            entity.HasIndex(x => x.SkuId).HasDatabaseName("IX_InventoryStocks_ProductSkuId");
            entity.HasOne(x => x.Warehouse).WithMany(x => x.Stocks).HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryStocks_Warehouses");
            entity.HasOne(x => x.Sku).WithMany().HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryStocks_ProductSkus");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.ToTable("InventoryTransactions", table => table.HasCheckConstraint("CK_InventoryTransactions_Type", "TransactionType IN ('In','Out','Reserve','Release','Adjust','Return')"));
            entity.HasKey(x => x.InventoryTransactionId).HasName("PK_InventoryTransactions");
            entity.Property(x => x.TransactionNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TransactionType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ReferenceType).HasMaxLength(50);
            entity.Property(x => x.Remark).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.TransactionNo).IsUnique().HasDatabaseName("UK_InventoryTransactions_TransactionNo");
            entity.HasIndex(x => new { x.SkuId, x.CreatedAt }).IsDescending(false, true).HasDatabaseName("IX_InventoryTransactions_SkuId_CreatedAt");
            entity.HasOne(x => x.Warehouse).WithMany(x => x.Transactions).HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryTransactions_Warehouses");
            entity.HasOne(x => x.Sku).WithMany().HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryTransactions_ProductSkus");
            entity.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryTransactions_CreatedBy");
        });

        modelBuilder.Entity<InventoryReservation>(entity =>
        {
            entity.ToTable("InventoryReservations", table =>
            {
                table.HasCheckConstraint("CK_InventoryReservations_ReservedQty", "ReservedQty > 0");
                table.HasCheckConstraint("CK_InventoryReservations_Status", "Status IN ('Reserved','Released','Consumed')");
            });
            entity.HasKey(x => x.ReservationId).HasName("PK_InventoryReservations");
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Reserved").IsRequired();
            entity.Property(x => x.ReservedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.Order).WithMany(x => x.InventoryReservations).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryReservations_Orders");
            entity.HasOne(x => x.OrderItem).WithMany(x => x.InventoryReservations).HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryReservations_OrderItems");
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryReservations_Warehouses");
            entity.HasOne(x => x.Sku).WithMany().HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_InventoryReservations_ProductSkus");
        });
    }

    private static void ConfigureOrders(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.ToTable("ShoppingCarts", table => table.HasCheckConstraint("CK_ShoppingCarts_CartStatus", "CartStatus IN ('Active','Converted','Abandoned')"));
            entity.HasKey(x => x.CartId).HasName("PK_ShoppingCarts");
            entity.Property(x => x.CartStatus).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ShoppingCarts_Users");
        });

        modelBuilder.Entity<ShoppingCartItem>(entity =>
        {
            entity.ToTable("ShoppingCartItems", table =>
            {
                table.HasCheckConstraint("CK_ShoppingCartItems_Quantity", "Quantity > 0");
                table.HasCheckConstraint("CK_ShoppingCartItems_UnitPrice", "UnitPrice >= 0");
            });
            entity.HasKey(x => x.CartItemId).HasName("PK_ShoppingCartItems");
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.CartId, x.SkuId }).IsUnique().HasDatabaseName("UK_ShoppingCartItems_CartId_SkuId");
            entity.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ShoppingCartItems_ShoppingCarts");
            entity.HasOne(x => x.Sku).WithMany().HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ShoppingCartItems_ProductSkus");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders", table =>
            {
                table.HasCheckConstraint("CK_Orders_OrderStatus", "OrderStatus IN ('Pending','Paid','Processing','Shipped','Completed','Cancelled')");
                table.HasCheckConstraint("CK_Orders_PaymentStatus", "PaymentStatus IN ('Pending','Paid','Failed','Refunded','PartialRefunded')");
                table.HasCheckConstraint("CK_Orders_ShippingStatus", "ShippingStatus IN ('Pending','Preparing','Shipped','Delivered','Returned')");
                table.HasCheckConstraint("CK_Orders_Amounts", "SubtotalAmount >= 0 AND DiscountAmount >= 0 AND ShippingFee >= 0 AND TotalAmount >= 0");
            });
            entity.HasKey(x => x.OrderId).HasName("PK_Orders");
            entity.Property(x => x.OrderNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OrderStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.ShippingStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            ConfigureMoney(entity.Property(x => x.SubtotalAmount).HasDefaultValue(0m));
            ConfigureMoney(entity.Property(x => x.DiscountAmount).HasDefaultValue(0m));
            ConfigureMoney(entity.Property(x => x.ShippingFee).HasDefaultValue(0m));
            ConfigureMoney(entity.Property(x => x.TotalAmount).HasDefaultValue(0m));
            entity.Property(x => x.ReceiverName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ReceiverPhone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ReceiverAddress).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Remark).HasMaxLength(500);
            entity.Property(x => x.OrderedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.OrderNo).IsUnique().HasDatabaseName("UK_Orders_OrderNo");
            entity.HasIndex(x => new { x.UserId, x.OrderedAt }).IsDescending(false, true).HasDatabaseName("IX_Orders_UserId_OrderedAt");
            entity.HasIndex(x => x.OrderStatus).HasDatabaseName("IX_Orders_OrderStatus");
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems", table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0");
                table.HasCheckConstraint("CK_OrderItems_Amount", "UnitPrice >= 0 AND DiscountAmount >= 0 AND SubtotalAmount >= 0");
            });
            entity.HasKey(x => x.OrderItemId).HasName("PK_OrderItems");
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SkuNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SkuNoSnapshot).HasMaxLength(50).IsRequired();
            ConfigureMoney(entity.Property(x => x.UnitPrice));
            ConfigureMoney(entity.Property(x => x.DiscountAmount).HasDefaultValue(0m));
            ConfigureMoney(entity.Property(x => x.SubtotalAmount));
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.OrderId).HasDatabaseName("IX_OrderItems_OrderId");
            entity.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderItems_Orders");
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderItems_Products");
            entity.HasOne(x => x.Sku).WithMany().HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderItems_ProductSkus");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("OrderStatusHistories");
            entity.HasKey(x => x.OrderStatusHistoryId).HasName("PK_OrderStatusHistories");
            entity.Property(x => x.OldStatus).HasMaxLength(30);
            entity.Property(x => x.NewStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ChangedReason).HasMaxLength(500);
            entity.Property(x => x.ChangedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.Order).WithMany(x => x.StatusHistories).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderStatusHistories_Orders");
            entity.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedBy).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderStatusHistories_ChangedBy");
        });
    }

    private static void ConfigurePayments(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments", table =>
            {
                table.HasCheckConstraint("CK_Payments_Method", "PaymentMethod IN ('CreditCard','ATM','COD','LinePay','ApplePay')");
                table.HasCheckConstraint("CK_Payments_Status", "PaymentStatus IN ('Pending','Paid','Failed','Cancelled','Refunded')");
                table.HasCheckConstraint("CK_Payments_Amount", "Amount >= 0");
            });
            entity.HasKey(x => x.PaymentId).HasName("PK_Payments");
            entity.Property(x => x.PaymentNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            ConfigureMoney(entity.Property(x => x.Amount));
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.PaymentNo).IsUnique().HasDatabaseName("UK_Payments_PaymentNo");
            entity.HasIndex(x => x.OrderId).HasDatabaseName("IX_Payments_OrderId");
            entity.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Payments_Orders");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("PaymentTransactions", table => table.HasCheckConstraint("CK_PaymentTransactions_Status", "TransactionStatus IN ('Success','Failed','Pending')"));
            entity.HasKey(x => x.PaymentTransactionId).HasName("PK_PaymentTransactions");
            entity.Property(x => x.ProviderName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ProviderTransactionNo).HasMaxLength(100);
            entity.Property(x => x.TransactionStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ErrorCode).HasMaxLength(50);
            entity.Property(x => x.ErrorMessage).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.ProviderTransactionNo).HasDatabaseName("IX_PaymentTransactions_ProviderTransactionNo");
            entity.HasOne(x => x.Payment).WithMany(x => x.Transactions).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_PaymentTransactions_Payments");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("Shipments", table => table.HasCheckConstraint("CK_Shipments_Status", "ShipmentStatus IN ('Preparing','Shipped','Delivered','Returned','Cancelled')"));
            entity.HasKey(x => x.ShipmentId).HasName("PK_Shipments");
            entity.Property(x => x.ShipmentNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CarrierName).HasMaxLength(100);
            entity.Property(x => x.TrackingNo).HasMaxLength(100);
            entity.Property(x => x.ShipmentStatus).HasMaxLength(30).HasDefaultValue("Preparing").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.ShipmentNo).IsUnique().HasDatabaseName("UK_Shipments_ShipmentNo");
            entity.HasIndex(x => x.TrackingNo).HasDatabaseName("IX_Shipments_TrackingNo");
            entity.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Shipments_Orders");
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Shipments_Warehouses");
        });

        modelBuilder.Entity<ShipmentItem>(entity =>
        {
            entity.ToTable("ShipmentItems", table => table.HasCheckConstraint("CK_ShipmentItems_Quantity", "Quantity > 0"));
            entity.HasKey(x => x.ShipmentItemId).HasName("PK_ShipmentItems");
            entity.HasOne(x => x.Shipment).WithMany(x => x.Items).HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ShipmentItems_Shipments");
            entity.HasOne(x => x.OrderItem).WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ShipmentItems_OrderItems");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.ToTable("Refunds", table =>
            {
                table.HasCheckConstraint("CK_Refunds_Status", "RefundStatus IN ('Requested','Approved','Rejected','Refunded')");
                table.HasCheckConstraint("CK_Refunds_Amount", "RefundAmount >= 0");
            });
            entity.HasKey(x => x.RefundId).HasName("PK_Refunds");
            entity.Property(x => x.RefundNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RefundStatus).HasMaxLength(30).HasDefaultValue("Requested").IsRequired();
            ConfigureMoney(entity.Property(x => x.RefundAmount));
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.RequestedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => x.RefundNo).IsUnique().HasDatabaseName("UK_Refunds_RefundNo");
            entity.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Refunds_Orders");
            entity.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Refunds_Payments");
            entity.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Refunds_CreatedBy");
        });

        modelBuilder.Entity<RefundItem>(entity =>
        {
            entity.ToTable("RefundItems", table =>
            {
                table.HasCheckConstraint("CK_RefundItems_Quantity", "Quantity > 0");
                table.HasCheckConstraint("CK_RefundItems_Amount", "RefundAmount >= 0");
            });
            entity.HasKey(x => x.RefundItemId).HasName("PK_RefundItems");
            ConfigureMoney(entity.Property(x => x.RefundAmount));
            entity.HasOne(x => x.Refund).WithMany(x => x.Items).HasForeignKey(x => x.RefundId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_RefundItems_Refunds");
            entity.HasOne(x => x.OrderItem).WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_RefundItems_OrderItems");
        });
    }

    private static void ConfigurePromotions(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupons", table =>
            {
                table.HasCheckConstraint("CK_Coupons_DiscountType", "DiscountType IN ('Amount','Percent')");
                table.HasCheckConstraint("CK_Coupons_Status", "Status IN ('Active','Inactive','Expired')");
                table.HasCheckConstraint("CK_Coupons_Amount", "DiscountValue >= 0 AND MinOrderAmount >= 0 AND UsedCount >= 0");
            });
            entity.HasKey(x => x.CouponId).HasName("PK_Coupons");
            entity.Property(x => x.CouponCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CouponName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DiscountType).HasMaxLength(30).IsRequired();
            ConfigureMoney(entity.Property(x => x.DiscountValue));
            ConfigureMoney(entity.Property(x => x.MinOrderAmount).HasDefaultValue(0m));
            ConfigureMoney(entity.Property(x => x.MaxDiscountAmount));
            entity.Property(x => x.UsedCount).HasDefaultValue(0);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(x => x.CouponCode).IsUnique().HasDatabaseName("UK_Coupons_CouponCode");
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.ToTable("CouponUsages", table => table.HasCheckConstraint("CK_CouponUsages_DiscountAmount", "DiscountAmount >= 0"));
            entity.HasKey(x => x.CouponUsageId).HasName("PK_CouponUsages");
            ConfigureMoney(entity.Property(x => x.DiscountAmount));
            entity.Property(x => x.UsedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.UserId, x.CouponId }).HasDatabaseName("IX_CouponUsages_UserId_CouponId");
            entity.HasOne(x => x.Coupon).WithMany(x => x.Usages).HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_CouponUsages_Coupons");
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_CouponUsages_Users");
            entity.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_CouponUsages_Orders");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.ToTable("Promotions", table =>
            {
                table.HasCheckConstraint("CK_Promotions_PromotionType", "PromotionType IN ('OrderDiscount','ProductDiscount','FreeShipping')");
                table.HasCheckConstraint("CK_Promotions_DiscountType", "DiscountType IN ('Amount','Percent')");
                table.HasCheckConstraint("CK_Promotions_Status", "Status IN ('Active','Inactive','Expired')");
                table.HasCheckConstraint("CK_Promotions_Amount", "DiscountValue >= 0 AND MinOrderAmount >= 0");
            });
            entity.HasKey(x => x.PromotionId).HasName("PK_Promotions");
            entity.Property(x => x.PromotionCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PromotionName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PromotionType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DiscountType).HasMaxLength(30).IsRequired();
            ConfigureMoney(entity.Property(x => x.DiscountValue));
            ConfigureMoney(entity.Property(x => x.MinOrderAmount).HasDefaultValue(0m));
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.HasIndex(x => x.PromotionCode).IsUnique().HasDatabaseName("UK_Promotions_PromotionCode");
        });

        modelBuilder.Entity<PromotionProduct>(entity =>
        {
            entity.ToTable("PromotionProducts", table => table.HasCheckConstraint("CK_PromotionProducts_Target", "ProductId IS NOT NULL OR SkuId IS NOT NULL"));
            entity.HasKey(x => x.PromotionProductId).HasName("PK_PromotionProducts");
            entity.HasOne(x => x.Promotion).WithMany(x => x.Products).HasForeignKey(x => x.PromotionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_PromotionProducts_Promotions");
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_PromotionProducts_Products");
            entity.HasOne(x => x.Sku).WithMany().HasForeignKey(x => x.SkuId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_PromotionProducts_ProductSkus");
        });

        modelBuilder.Entity<OrderDiscount>(entity =>
        {
            entity.ToTable("OrderDiscounts", table =>
            {
                table.HasCheckConstraint("CK_OrderDiscounts_SourceType", "DiscountSourceType IN ('Coupon','Promotion','Manual')");
                table.HasCheckConstraint("CK_OrderDiscounts_Amount", "DiscountAmount >= 0");
            });
            entity.HasKey(x => x.OrderDiscountId).HasName("PK_OrderDiscounts");
            entity.Property(x => x.DiscountSourceType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DiscountName).HasMaxLength(100).IsRequired();
            ConfigureMoney(entity.Property(x => x.DiscountAmount));
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderDiscounts_Orders");
            entity.HasOne(x => x.Coupon).WithMany(x => x.OrderDiscounts).HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderDiscounts_Coupons");
            entity.HasOne(x => x.Promotion).WithMany(x => x.OrderDiscounts).HasForeignKey(x => x.PromotionId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_OrderDiscounts_Promotions");
        });
    }

    private static void ConfigureLogs(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", table => table.HasCheckConstraint("CK_AuditLogs_ActionType", "ActionType IN ('Insert','Update','Delete')"));
            entity.HasKey(x => x.AuditLogId).HasName("PK_AuditLogs");
            entity.Property(x => x.TableName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ActionType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(50);
            entity.Property(x => x.ChangedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.TableName, x.RecordId }).HasDatabaseName("IX_AuditLogs_TableName_RecordId");
            entity.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedBy).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_AuditLogs_Users");
        });

        modelBuilder.Entity<AdminActionLog>(entity =>
        {
            entity.ToTable("AdminActionLogs");
            entity.HasKey(x => x.AdminActionLogId).HasName("PK_AdminActionLogs");
            entity.Property(x => x.ModuleName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ActionName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(100);
            entity.Property(x => x.TargetId).HasMaxLength(100);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IpAddress).HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(x => new { x.UserId, x.CreatedAt }).IsDescending(false, true).HasDatabaseName("IX_AdminActionLogs_UserId_CreatedAt");
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_AdminActionLogs_Users");
        });

        modelBuilder.Entity<SystemErrorLog>(entity =>
        {
            entity.ToTable("SystemErrorLogs", table => table.HasCheckConstraint("CK_SystemErrorLogs_ErrorLevel", "ErrorLevel IN ('Info','Warning','Error','Critical')"));
            entity.HasKey(x => x.ErrorLogId).HasName("PK_SystemErrorLogs");
            entity.Property(x => x.ErrorLevel).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(200);
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.RequestPath).HasMaxLength(500);
            entity.Property(x => x.IpAddress).HasMaxLength(50);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_SystemErrorLogs_Users");
        });
    }

    private static PropertyBuilder<decimal> ConfigureMoney(PropertyBuilder<decimal> propertyBuilder)
    {
        return propertyBuilder.HasPrecision(18, 2);
    }

    private static PropertyBuilder<decimal?> ConfigureMoney(PropertyBuilder<decimal?> propertyBuilder)
    {
        return propertyBuilder.HasPrecision(18, 2);
    }
}
