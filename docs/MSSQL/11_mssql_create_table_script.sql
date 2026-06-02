/*
    MSSQL 電商後台資料庫建表範例
    Database: EcommerceBackendDB
    說明：此 SQL 為教學與專案初始設計版本，可依實際需求調整。
*/

CREATE DATABASE EcommerceBackendDB;
GO

USE EcommerceBackendDB;
GO

/* =========================
   1. 帳戶與權限模組
========================= */

CREATE TABLE Users (
    UserId BIGINT IDENTITY(1,1) NOT NULL,
    UserNo NVARCHAR(30) NOT NULL,
    Account NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NULL,
    Phone NVARCHAR(30) NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    PasswordSalt NVARCHAR(200) NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    UserType NVARCHAR(30) NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Active',
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UK_Users_UserNo UNIQUE (UserNo),
    CONSTRAINT UK_Users_Account UNIQUE (Account),
    CONSTRAINT UK_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_UserType CHECK (UserType IN ('Customer','Admin','Staff')),
    CONSTRAINT CK_Users_Status CHECK (Status IN ('Active','Inactive','Locked'))
);
GO

CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) NOT NULL,
    RoleCode NVARCHAR(50) NOT NULL,
    RoleName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT UK_Roles_RoleCode UNIQUE (RoleCode)
);
GO

CREATE TABLE Permissions (
    PermissionId INT IDENTITY(1,1) NOT NULL,
    PermissionCode NVARCHAR(100) NOT NULL,
    PermissionName NVARCHAR(100) NOT NULL,
    ModuleName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Permissions PRIMARY KEY (PermissionId),
    CONSTRAINT UK_Permissions_PermissionCode UNIQUE (PermissionCode)
);
GO

CREATE TABLE UserRoles (
    UserRoleId BIGINT IDENTITY(1,1) NOT NULL,
    UserId BIGINT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    AssignedBy BIGINT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserRoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT FK_UserRoles_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(UserId),
    CONSTRAINT UK_UserRoles_UserId_RoleId UNIQUE (UserId, RoleId)
);
GO

CREATE TABLE RolePermissions (
    RolePermissionId BIGINT IDENTITY(1,1) NOT NULL,
    RoleId INT NOT NULL,
    PermissionId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RolePermissionId),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId),
    CONSTRAINT UK_RolePermissions_RoleId_PermissionId UNIQUE (RoleId, PermissionId)
);
GO

CREATE TABLE UserLoginLogs (
    LoginLogId BIGINT IDENTITY(1,1) NOT NULL,
    UserId BIGINT NULL,
    Account NVARCHAR(100) NULL,
    LoginResult NVARCHAR(30) NOT NULL,
    FailureReason NVARCHAR(300) NULL,
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    LoginAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_UserLoginLogs PRIMARY KEY (LoginLogId),
    CONSTRAINT FK_UserLoginLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_UserLoginLogs_LoginResult CHECK (LoginResult IN ('Success','Failed','Logout'))
);
GO

/* =========================
   2. 商品模組
========================= */

CREATE TABLE ProductCategories (
    CategoryId INT IDENTITY(1,1) NOT NULL,
    ParentCategoryId INT NULL,
    CategoryCode NVARCHAR(50) NOT NULL,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_ProductCategories PRIMARY KEY (CategoryId),
    CONSTRAINT FK_ProductCategories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(CategoryId),
    CONSTRAINT UK_ProductCategories_CategoryCode UNIQUE (CategoryCode)
);
GO

CREATE TABLE Products (
    ProductId BIGINT IDENTITY(1,1) NOT NULL,
    ProductNo NVARCHAR(30) NOT NULL,
    CategoryId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    BrandName NVARCHAR(100) NULL,
    ShortDescription NVARCHAR(500) NULL,
    FullDescription NVARCHAR(MAX) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Draft',
    IsFeatured BIT NOT NULL DEFAULT 0,
    PublishedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Products PRIMARY KEY (ProductId),
    CONSTRAINT FK_Products_ProductCategories FOREIGN KEY (CategoryId) REFERENCES ProductCategories(CategoryId),
    CONSTRAINT UK_Products_ProductNo UNIQUE (ProductNo),
    CONSTRAINT CK_Products_Status CHECK (Status IN ('Draft','Active','Inactive','Archived'))
);
GO

CREATE TABLE ProductSkus (
    SkuId BIGINT IDENTITY(1,1) NOT NULL,
    ProductId BIGINT NOT NULL,
    SkuNo NVARCHAR(50) NOT NULL,
    Barcode NVARCHAR(100) NULL,
    SkuName NVARCHAR(200) NOT NULL,
    SpecText NVARCHAR(500) NULL,
    ListPrice DECIMAL(18,2) NOT NULL,
    SalePrice DECIMAL(18,2) NOT NULL,
    CostPrice DECIMAL(18,2) NULL,
    Weight DECIMAL(18,3) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_ProductSkus PRIMARY KEY (SkuId),
    CONSTRAINT FK_ProductSkus_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT UK_ProductSkus_SkuNo UNIQUE (SkuNo),
    CONSTRAINT UK_ProductSkus_Barcode UNIQUE (Barcode),
    CONSTRAINT CK_ProductSkus_ListPrice CHECK (ListPrice >= 0),
    CONSTRAINT CK_ProductSkus_SalePrice CHECK (SalePrice >= 0),
    CONSTRAINT CK_ProductSkus_CostPrice CHECK (CostPrice IS NULL OR CostPrice >= 0),
    CONSTRAINT CK_ProductSkus_Status CHECK (Status IN ('Active','Inactive'))
);
GO

CREATE TABLE ProductImages (
    ImageId BIGINT IDENTITY(1,1) NOT NULL,
    ProductId BIGINT NOT NULL,
    SkuId BIGINT NULL,
    ImageUrl NVARCHAR(1000) NOT NULL,
    AltText NVARCHAR(200) NULL,
    IsMainImage BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ProductImages PRIMARY KEY (ImageId),
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_ProductImages_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId)
);
GO

CREATE TABLE ProductAttributes (
    AttributeId INT IDENTITY(1,1) NOT NULL,
    AttributeCode NVARCHAR(50) NOT NULL,
    AttributeName NVARCHAR(100) NOT NULL,
    InputType NVARCHAR(30) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ProductAttributes PRIMARY KEY (AttributeId),
    CONSTRAINT UK_ProductAttributes_AttributeCode UNIQUE (AttributeCode),
    CONSTRAINT CK_ProductAttributes_InputType CHECK (InputType IN ('Text','Select','Number'))
);
GO

CREATE TABLE ProductAttributeValues (
    AttributeValueId BIGINT IDENTITY(1,1) NOT NULL,
    SkuId BIGINT NOT NULL,
    AttributeId INT NOT NULL,
    AttributeValue NVARCHAR(200) NOT NULL,
    CONSTRAINT PK_ProductAttributeValues PRIMARY KEY (AttributeValueId),
    CONSTRAINT FK_ProductAttributeValues_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId),
    CONSTRAINT FK_ProductAttributeValues_ProductAttributes FOREIGN KEY (AttributeId) REFERENCES ProductAttributes(AttributeId),
    CONSTRAINT UK_ProductAttributeValues_SkuId_AttributeId UNIQUE (SkuId, AttributeId)
);
GO

/* =========================
   3. 庫存模組
========================= */

CREATE TABLE Warehouses (
    WarehouseId INT IDENTITY(1,1) NOT NULL,
    WarehouseCode NVARCHAR(50) NOT NULL,
    WarehouseName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(500) NULL,
    ContactName NVARCHAR(100) NULL,
    ContactPhone NVARCHAR(30) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Warehouses PRIMARY KEY (WarehouseId),
    CONSTRAINT UK_Warehouses_WarehouseCode UNIQUE (WarehouseCode)
);
GO

CREATE TABLE InventoryStocks (
    InventoryStockId BIGINT IDENTITY(1,1) NOT NULL,
    WarehouseId INT NOT NULL,
    SkuId BIGINT NOT NULL,
    OnHandQty INT NOT NULL DEFAULT 0,
    ReservedQty INT NOT NULL DEFAULT 0,
    AvailableQty AS (OnHandQty - ReservedQty),
    SafetyStockQty INT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_InventoryStocks PRIMARY KEY (InventoryStockId),
    CONSTRAINT FK_InventoryStocks_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_InventoryStocks_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId),
    CONSTRAINT UK_InventoryStocks_WarehouseId_SkuId UNIQUE (WarehouseId, SkuId),
    CONSTRAINT CK_InventoryStocks_OnHandQty CHECK (OnHandQty >= 0),
    CONSTRAINT CK_InventoryStocks_ReservedQty CHECK (ReservedQty >= 0),
    CONSTRAINT CK_InventoryStocks_SafetyStockQty CHECK (SafetyStockQty >= 0),
    CONSTRAINT CK_InventoryStocks_AvailableLogic CHECK (OnHandQty >= ReservedQty)
);
GO

/* Orders 會在後面建立，因此 InventoryReservations 之後再建立 */

CREATE TABLE InventoryTransactions (
    InventoryTransactionId BIGINT IDENTITY(1,1) NOT NULL,
    TransactionNo NVARCHAR(50) NOT NULL,
    WarehouseId INT NOT NULL,
    SkuId BIGINT NOT NULL,
    TransactionType NVARCHAR(30) NOT NULL,
    Quantity INT NOT NULL,
    BeforeQty INT NOT NULL,
    AfterQty INT NOT NULL,
    ReferenceType NVARCHAR(50) NULL,
    ReferenceId BIGINT NULL,
    Remark NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy BIGINT NULL,
    CONSTRAINT PK_InventoryTransactions PRIMARY KEY (InventoryTransactionId),
    CONSTRAINT UK_InventoryTransactions_TransactionNo UNIQUE (TransactionNo),
    CONSTRAINT FK_InventoryTransactions_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_InventoryTransactions_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId),
    CONSTRAINT FK_InventoryTransactions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_InventoryTransactions_Type CHECK (TransactionType IN ('In','Out','Reserve','Release','Adjust','Return'))
);
GO

/* =========================
   4. 購物車與訂單模組
========================= */

CREATE TABLE ShoppingCarts (
    CartId BIGINT IDENTITY(1,1) NOT NULL,
    UserId BIGINT NOT NULL,
    CartStatus NVARCHAR(30) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_ShoppingCarts PRIMARY KEY (CartId),
    CONSTRAINT FK_ShoppingCarts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_ShoppingCarts_CartStatus CHECK (CartStatus IN ('Active','Converted','Abandoned'))
);
GO

CREATE TABLE ShoppingCartItems (
    CartItemId BIGINT IDENTITY(1,1) NOT NULL,
    CartId BIGINT NOT NULL,
    SkuId BIGINT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT PK_ShoppingCartItems PRIMARY KEY (CartItemId),
    CONSTRAINT FK_ShoppingCartItems_ShoppingCarts FOREIGN KEY (CartId) REFERENCES ShoppingCarts(CartId),
    CONSTRAINT FK_ShoppingCartItems_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId),
    CONSTRAINT UK_ShoppingCartItems_CartId_SkuId UNIQUE (CartId, SkuId),
    CONSTRAINT CK_ShoppingCartItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_ShoppingCartItems_UnitPrice CHECK (UnitPrice >= 0)
);
GO

CREATE TABLE Orders (
    OrderId BIGINT IDENTITY(1,1) NOT NULL,
    OrderNo NVARCHAR(50) NOT NULL,
    UserId BIGINT NOT NULL,
    OrderStatus NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    PaymentStatus NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    ShippingStatus NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    SubtotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ReceiverName NVARCHAR(100) NOT NULL,
    ReceiverPhone NVARCHAR(30) NOT NULL,
    ReceiverAddress NVARCHAR(500) NOT NULL,
    Remark NVARCHAR(500) NULL,
    OrderedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PaidAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    CancelledAt DATETIME2 NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
    CONSTRAINT UK_Orders_OrderNo UNIQUE (OrderNo),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Orders_OrderStatus CHECK (OrderStatus IN ('Pending','Paid','Processing','Shipped','Completed','Cancelled')),
    CONSTRAINT CK_Orders_PaymentStatus CHECK (PaymentStatus IN ('Pending','Paid','Failed','Refunded','PartialRefunded')),
    CONSTRAINT CK_Orders_ShippingStatus CHECK (ShippingStatus IN ('Pending','Preparing','Shipped','Delivered','Returned')),
    CONSTRAINT CK_Orders_Amounts CHECK (SubtotalAmount >= 0 AND DiscountAmount >= 0 AND ShippingFee >= 0 AND TotalAmount >= 0)
);
GO

CREATE TABLE OrderItems (
    OrderItemId BIGINT IDENTITY(1,1) NOT NULL,
    OrderId BIGINT NOT NULL,
    ProductId BIGINT NOT NULL,
    SkuId BIGINT NOT NULL,
    ProductNameSnapshot NVARCHAR(200) NOT NULL,
    SkuNameSnapshot NVARCHAR(200) NOT NULL,
    SkuNoSnapshot NVARCHAR(50) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    SubtotalAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_OrderItems PRIMARY KEY (OrderItemId),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_OrderItems_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId),
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderItems_Amount CHECK (UnitPrice >= 0 AND DiscountAmount >= 0 AND SubtotalAmount >= 0)
);
GO

CREATE TABLE OrderStatusHistories (
    OrderStatusHistoryId BIGINT IDENTITY(1,1) NOT NULL,
    OrderId BIGINT NOT NULL,
    OldStatus NVARCHAR(30) NULL,
    NewStatus NVARCHAR(30) NOT NULL,
    ChangedReason NVARCHAR(500) NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ChangedBy BIGINT NULL,
    CONSTRAINT PK_OrderStatusHistories PRIMARY KEY (OrderStatusHistoryId),
    CONSTRAINT FK_OrderStatusHistories_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderStatusHistories_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Users(UserId)
);
GO

CREATE TABLE InventoryReservations (
    ReservationId BIGINT IDENTITY(1,1) NOT NULL,
    OrderId BIGINT NOT NULL,
    OrderItemId BIGINT NOT NULL,
    WarehouseId INT NOT NULL,
    SkuId BIGINT NOT NULL,
    ReservedQty INT NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Reserved',
    ReservedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ReleasedAt DATETIME2 NULL,
    ConsumedAt DATETIME2 NULL,
    CONSTRAINT PK_InventoryReservations PRIMARY KEY (ReservationId),
    CONSTRAINT FK_InventoryReservations_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_InventoryReservations_OrderItems FOREIGN KEY (OrderItemId) REFERENCES OrderItems(OrderItemId),
    CONSTRAINT FK_InventoryReservations_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT FK_InventoryReservations_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId),
    CONSTRAINT CK_InventoryReservations_ReservedQty CHECK (ReservedQty > 0),
    CONSTRAINT CK_InventoryReservations_Status CHECK (Status IN ('Reserved','Released','Consumed'))
);
GO

/* =========================
   5. 付款、物流與退款模組
========================= */

CREATE TABLE Payments (
    PaymentId BIGINT IDENTITY(1,1) NOT NULL,
    PaymentNo NVARCHAR(50) NOT NULL,
    OrderId BIGINT NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    PaymentStatus NVARCHAR(30) NOT NULL DEFAULT 'Pending',
    Amount DECIMAL(18,2) NOT NULL,
    PaidAt DATETIME2 NULL,
    ExpiredAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Payments PRIMARY KEY (PaymentId),
    CONSTRAINT UK_Payments_PaymentNo UNIQUE (PaymentNo),
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT CK_Payments_Method CHECK (PaymentMethod IN ('CreditCard','ATM','COD','LinePay','ApplePay')),
    CONSTRAINT CK_Payments_Status CHECK (PaymentStatus IN ('Pending','Paid','Failed','Cancelled','Refunded')),
    CONSTRAINT CK_Payments_Amount CHECK (Amount >= 0)
);
GO

CREATE TABLE PaymentTransactions (
    PaymentTransactionId BIGINT IDENTITY(1,1) NOT NULL,
    PaymentId BIGINT NOT NULL,
    ProviderName NVARCHAR(100) NOT NULL,
    ProviderTransactionNo NVARCHAR(100) NULL,
    TransactionStatus NVARCHAR(30) NOT NULL,
    RequestPayload NVARCHAR(MAX) NULL,
    ResponsePayload NVARCHAR(MAX) NULL,
    ErrorCode NVARCHAR(50) NULL,
    ErrorMessage NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PaymentTransactions PRIMARY KEY (PaymentTransactionId),
    CONSTRAINT FK_PaymentTransactions_Payments FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId),
    CONSTRAINT CK_PaymentTransactions_Status CHECK (TransactionStatus IN ('Success','Failed','Pending'))
);
GO

CREATE TABLE Shipments (
    ShipmentId BIGINT IDENTITY(1,1) NOT NULL,
    ShipmentNo NVARCHAR(50) NOT NULL,
    OrderId BIGINT NOT NULL,
    WarehouseId INT NULL,
    CarrierName NVARCHAR(100) NULL,
    TrackingNo NVARCHAR(100) NULL,
    ShipmentStatus NVARCHAR(30) NOT NULL DEFAULT 'Preparing',
    ShippedAt DATETIME2 NULL,
    DeliveredAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Shipments PRIMARY KEY (ShipmentId),
    CONSTRAINT UK_Shipments_ShipmentNo UNIQUE (ShipmentNo),
    CONSTRAINT FK_Shipments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_Shipments_Warehouses FOREIGN KEY (WarehouseId) REFERENCES Warehouses(WarehouseId),
    CONSTRAINT CK_Shipments_Status CHECK (ShipmentStatus IN ('Preparing','Shipped','Delivered','Returned','Cancelled'))
);
GO

CREATE TABLE ShipmentItems (
    ShipmentItemId BIGINT IDENTITY(1,1) NOT NULL,
    ShipmentId BIGINT NOT NULL,
    OrderItemId BIGINT NOT NULL,
    Quantity INT NOT NULL,
    CONSTRAINT PK_ShipmentItems PRIMARY KEY (ShipmentItemId),
    CONSTRAINT FK_ShipmentItems_Shipments FOREIGN KEY (ShipmentId) REFERENCES Shipments(ShipmentId),
    CONSTRAINT FK_ShipmentItems_OrderItems FOREIGN KEY (OrderItemId) REFERENCES OrderItems(OrderItemId),
    CONSTRAINT CK_ShipmentItems_Quantity CHECK (Quantity > 0)
);
GO

CREATE TABLE Refunds (
    RefundId BIGINT IDENTITY(1,1) NOT NULL,
    RefundNo NVARCHAR(50) NOT NULL,
    OrderId BIGINT NOT NULL,
    PaymentId BIGINT NULL,
    RefundStatus NVARCHAR(30) NOT NULL DEFAULT 'Requested',
    RefundAmount DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(500) NULL,
    RequestedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ApprovedAt DATETIME2 NULL,
    RefundedAt DATETIME2 NULL,
    CreatedBy BIGINT NULL,
    CONSTRAINT PK_Refunds PRIMARY KEY (RefundId),
    CONSTRAINT UK_Refunds_RefundNo UNIQUE (RefundNo),
    CONSTRAINT FK_Refunds_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_Refunds_Payments FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId),
    CONSTRAINT FK_Refunds_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_Refunds_Status CHECK (RefundStatus IN ('Requested','Approved','Rejected','Refunded')),
    CONSTRAINT CK_Refunds_Amount CHECK (RefundAmount >= 0)
);
GO

CREATE TABLE RefundItems (
    RefundItemId BIGINT IDENTITY(1,1) NOT NULL,
    RefundId BIGINT NOT NULL,
    OrderItemId BIGINT NOT NULL,
    Quantity INT NOT NULL,
    RefundAmount DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_RefundItems PRIMARY KEY (RefundItemId),
    CONSTRAINT FK_RefundItems_Refunds FOREIGN KEY (RefundId) REFERENCES Refunds(RefundId),
    CONSTRAINT FK_RefundItems_OrderItems FOREIGN KEY (OrderItemId) REFERENCES OrderItems(OrderItemId),
    CONSTRAINT CK_RefundItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_RefundItems_Amount CHECK (RefundAmount >= 0)
);
GO

/* =========================
   6. 優惠與促銷模組
========================= */

CREATE TABLE Coupons (
    CouponId BIGINT IDENTITY(1,1) NOT NULL,
    CouponCode NVARCHAR(50) NOT NULL,
    CouponName NVARCHAR(100) NOT NULL,
    DiscountType NVARCHAR(30) NOT NULL,
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxDiscountAmount DECIMAL(18,2) NULL,
    TotalUsageLimit INT NULL,
    PerUserUsageLimit INT NULL,
    UsedCount INT NOT NULL DEFAULT 0,
    StartAt DATETIME2 NOT NULL,
    EndAt DATETIME2 NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Coupons PRIMARY KEY (CouponId),
    CONSTRAINT UK_Coupons_CouponCode UNIQUE (CouponCode),
    CONSTRAINT CK_Coupons_DiscountType CHECK (DiscountType IN ('Amount','Percent')),
    CONSTRAINT CK_Coupons_Status CHECK (Status IN ('Active','Inactive','Expired')),
    CONSTRAINT CK_Coupons_Amount CHECK (DiscountValue >= 0 AND MinOrderAmount >= 0 AND UsedCount >= 0)
);
GO

CREATE TABLE CouponUsages (
    CouponUsageId BIGINT IDENTITY(1,1) NOT NULL,
    CouponId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    OrderId BIGINT NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    UsedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CouponUsages PRIMARY KEY (CouponUsageId),
    CONSTRAINT FK_CouponUsages_Coupons FOREIGN KEY (CouponId) REFERENCES Coupons(CouponId),
    CONSTRAINT FK_CouponUsages_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_CouponUsages_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT CK_CouponUsages_DiscountAmount CHECK (DiscountAmount >= 0)
);
GO

CREATE TABLE Promotions (
    PromotionId BIGINT IDENTITY(1,1) NOT NULL,
    PromotionCode NVARCHAR(50) NOT NULL,
    PromotionName NVARCHAR(100) NOT NULL,
    PromotionType NVARCHAR(30) NOT NULL,
    DiscountType NVARCHAR(30) NOT NULL,
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    StartAt DATETIME2 NOT NULL,
    EndAt DATETIME2 NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Active',
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Promotions PRIMARY KEY (PromotionId),
    CONSTRAINT UK_Promotions_PromotionCode UNIQUE (PromotionCode),
    CONSTRAINT CK_Promotions_PromotionType CHECK (PromotionType IN ('OrderDiscount','ProductDiscount','FreeShipping')),
    CONSTRAINT CK_Promotions_DiscountType CHECK (DiscountType IN ('Amount','Percent')),
    CONSTRAINT CK_Promotions_Status CHECK (Status IN ('Active','Inactive','Expired')),
    CONSTRAINT CK_Promotions_Amount CHECK (DiscountValue >= 0 AND MinOrderAmount >= 0)
);
GO

CREATE TABLE PromotionProducts (
    PromotionProductId BIGINT IDENTITY(1,1) NOT NULL,
    PromotionId BIGINT NOT NULL,
    ProductId BIGINT NULL,
    SkuId BIGINT NULL,
    CONSTRAINT PK_PromotionProducts PRIMARY KEY (PromotionProductId),
    CONSTRAINT FK_PromotionProducts_Promotions FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId),
    CONSTRAINT FK_PromotionProducts_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_PromotionProducts_ProductSkus FOREIGN KEY (SkuId) REFERENCES ProductSkus(SkuId),
    CONSTRAINT CK_PromotionProducts_Target CHECK (ProductId IS NOT NULL OR SkuId IS NOT NULL)
);
GO

CREATE TABLE OrderDiscounts (
    OrderDiscountId BIGINT IDENTITY(1,1) NOT NULL,
    OrderId BIGINT NOT NULL,
    DiscountSourceType NVARCHAR(30) NOT NULL,
    CouponId BIGINT NULL,
    PromotionId BIGINT NULL,
    DiscountName NVARCHAR(100) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_OrderDiscounts PRIMARY KEY (OrderDiscountId),
    CONSTRAINT FK_OrderDiscounts_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_OrderDiscounts_Coupons FOREIGN KEY (CouponId) REFERENCES Coupons(CouponId),
    CONSTRAINT FK_OrderDiscounts_Promotions FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId),
    CONSTRAINT CK_OrderDiscounts_SourceType CHECK (DiscountSourceType IN ('Coupon','Promotion','Manual')),
    CONSTRAINT CK_OrderDiscounts_Amount CHECK (DiscountAmount >= 0)
);
GO

/* =========================
   7. 稽核與系統紀錄模組
========================= */

CREATE TABLE AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) NOT NULL,
    TableName NVARCHAR(100) NOT NULL,
    RecordId NVARCHAR(100) NOT NULL,
    ActionType NVARCHAR(30) NOT NULL,
    OldValueJson NVARCHAR(MAX) NULL,
    NewValueJson NVARCHAR(MAX) NULL,
    ChangedBy BIGINT NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IpAddress NVARCHAR(50) NULL,
    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditLogId),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (ChangedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_AuditLogs_ActionType CHECK (ActionType IN ('Insert','Update','Delete'))
);
GO

CREATE TABLE AdminActionLogs (
    AdminActionLogId BIGINT IDENTITY(1,1) NOT NULL,
    UserId BIGINT NOT NULL,
    ModuleName NVARCHAR(100) NOT NULL,
    ActionName NVARCHAR(100) NOT NULL,
    TargetType NVARCHAR(100) NULL,
    TargetId NVARCHAR(100) NULL,
    Description NVARCHAR(1000) NULL,
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AdminActionLogs PRIMARY KEY (AdminActionLogId),
    CONSTRAINT FK_AdminActionLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

CREATE TABLE SystemErrorLogs (
    ErrorLogId BIGINT IDENTITY(1,1) NOT NULL,
    ErrorLevel NVARCHAR(30) NOT NULL,
    Source NVARCHAR(200) NULL,
    Message NVARCHAR(1000) NOT NULL,
    StackTrace NVARCHAR(MAX) NULL,
    RequestPath NVARCHAR(500) NULL,
    RequestBody NVARCHAR(MAX) NULL,
    UserId BIGINT NULL,
    IpAddress NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_SystemErrorLogs PRIMARY KEY (ErrorLogId),
    CONSTRAINT FK_SystemErrorLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_SystemErrorLogs_ErrorLevel CHECK (ErrorLevel IN ('Info','Warning','Error','Critical'))
);
GO

/* =========================
   8. Indexes
========================= */

CREATE INDEX IX_Users_Account ON Users(Account);
CREATE INDEX IX_Products_ProductName ON Products(ProductName);
CREATE INDEX IX_ProductSkus_ProductId ON ProductSkus(ProductId);
CREATE INDEX IX_Orders_UserId_OrderedAt ON Orders(UserId, OrderedAt DESC);
CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_InventoryTransactions_SkuId_CreatedAt ON InventoryTransactions(SkuId, CreatedAt DESC);
CREATE INDEX IX_Payments_OrderId ON Payments(OrderId);
CREATE INDEX IX_PaymentTransactions_ProviderTransactionNo ON PaymentTransactions(ProviderTransactionNo);
CREATE INDEX IX_Shipments_TrackingNo ON Shipments(TrackingNo);
CREATE INDEX IX_CouponUsages_UserId_CouponId ON CouponUsages(UserId, CouponId);
CREATE INDEX IX_AuditLogs_TableName_RecordId ON AuditLogs(TableName, RecordId);
CREATE INDEX IX_AdminActionLogs_UserId_CreatedAt ON AdminActionLogs(UserId, CreatedAt DESC);
GO

/* =========================
   9. 預設角色與權限範例
========================= */

INSERT INTO Roles (RoleCode, RoleName, Description, IsSystemRole)
VALUES
('SUPER_ADMIN', N'超級管理員', N'擁有全部後台權限', 1),
('PRODUCT_MANAGER', N'商品管理員', N'管理商品、分類與 SKU', 1),
('ORDER_MANAGER', N'訂單管理員', N'管理訂單、付款與退款', 1),
('WAREHOUSE_STAFF', N'倉管人員', N'管理庫存與出貨', 1),
('MARKETING_STAFF', N'行銷人員', N'管理優惠券與促銷', 1);
GO

INSERT INTO Permissions (PermissionCode, PermissionName, ModuleName, Description)
VALUES
('PRODUCT_VIEW', N'檢視商品', 'Product', N'可查詢商品資料'),
('PRODUCT_CREATE', N'新增商品', 'Product', N'可新增商品'),
('PRODUCT_UPDATE', N'修改商品', 'Product', N'可修改商品'),
('PRODUCT_DELETE', N'刪除商品', 'Product', N'可刪除或封存商品'),
('ORDER_VIEW', N'檢視訂單', 'Order', N'可查詢訂單'),
('ORDER_UPDATE_STATUS', N'修改訂單狀態', 'Order', N'可變更訂單狀態'),
('INVENTORY_VIEW', N'檢視庫存', 'Inventory', N'可查詢庫存'),
('INVENTORY_ADJUST', N'調整庫存', 'Inventory', N'可調整庫存'),
('COUPON_MANAGE', N'管理優惠券', 'Promotion', N'可管理優惠券與促銷'),
('AUDIT_VIEW', N'檢視稽核紀錄', 'Audit', N'可查詢後台操作與異動紀錄');
GO
