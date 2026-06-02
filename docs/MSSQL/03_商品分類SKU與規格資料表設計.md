# 03 商品、分類、SKU 與規格資料表設計

## 1. 模組目的

商品目錄模組是電商系統的核心，負責管理商品分類、商品主檔、商品 SKU、商品圖片、商品屬性與商品狀態。

電商系統中，商品設計最重要的觀念是：

```text
Product 商品主檔：負責展示與共同資訊
Sku 銷售規格：負責價格、庫存、條碼、實際銷售單位
```

例如：

```text
商品：經典白色 T-shirt
SKU 1：白色 / M 號
SKU 2：白色 / L 號
SKU 3：白色 / XL 號
```

因此，不建議只用一張 `Products` 表記錄所有商品資料。

---

## 2. ProductCategories 商品分類表

### 2.1 用途

`ProductCategories` 用於管理商品分類，支援多層分類，例如：

```text
服飾
├─ 男裝
│  ├─ 上衣
│  └─ 褲子
└─ 女裝
   ├─ 洋裝
   └─ 外套
```

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| CategoryId | INT IDENTITY | PK | N | 分類主鍵 |
| ParentCategoryId | INT | FK | Y | 上層分類 ID |
| CategoryCode | NVARCHAR(50) | UK | N | 分類代碼 |
| CategoryName | NVARCHAR(100) |  | N | 分類名稱 |
| Description | NVARCHAR(500) |  | Y | 分類說明 |
| SortOrder | INT |  | N | 排序 |
| IsActive | BIT |  | N | 是否啟用 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 2.3 關聯設計

```text
ProductCategories.ParentCategoryId -> ProductCategories.CategoryId
Products.CategoryId -> ProductCategories.CategoryId
```

---

## 3. Products 商品主檔

### 3.1 用途

`Products` 用於保存商品共同資訊，例如商品名稱、說明、品牌、分類、上下架狀態。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ProductId | BIGINT IDENTITY | PK | N | 商品主鍵 |
| ProductNo | NVARCHAR(30) | UK/TK | N | 商品編號，例如 PRD202605170001 |
| CategoryId | INT | FK | N | 商品分類 |
| ProductName | NVARCHAR(200) | IDX | N | 商品名稱 |
| BrandName | NVARCHAR(100) |  | Y | 品牌名稱 |
| ShortDescription | NVARCHAR(500) |  | Y | 短描述 |
| FullDescription | NVARCHAR(MAX) |  | Y | 完整描述 |
| Status | NVARCHAR(30) | CK | N | Draft/Active/Inactive/Archived |
| IsFeatured | BIT |  | N | 是否推薦商品 |
| PublishedAt | DATETIME2 |  | Y | 上架時間 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 3.3 商品狀態建議

| 狀態 | 說明 |
|---|---|
| Draft | 草稿，尚未上架 |
| Active | 已上架，可販售 |
| Inactive | 下架，不可販售 |
| Archived | 封存，不再使用 |

---

## 4. ProductSkus 商品 SKU 表

### 4.1 用途

`ProductSkus` 是實際銷售單位，通常會包含價格、規格、條碼與銷售狀態。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| SkuId | BIGINT IDENTITY | PK | N | SKU 主鍵 |
| ProductId | BIGINT | FK | N | 商品 ID |
| SkuNo | NVARCHAR(50) | UK/TK | N | SKU 編號 |
| Barcode | NVARCHAR(100) | UK | Y | 條碼 |
| SkuName | NVARCHAR(200) |  | N | SKU 名稱 |
| SpecText | NVARCHAR(500) |  | Y | 規格文字，例如 白色/M |
| ListPrice | DECIMAL(18,2) | CK | N | 原價 |
| SalePrice | DECIMAL(18,2) | CK | N | 售價 |
| CostPrice | DECIMAL(18,2) |  | Y | 成本價 |
| Weight | DECIMAL(18,3) |  | Y | 重量 |
| Status | NVARCHAR(30) | CK | N | Active/Inactive |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 4.3 價格檢查限制

```text
ListPrice >= 0
SalePrice >= 0
CostPrice >= 0
```

---

## 5. ProductImages 商品圖片表

### 5.1 用途

商品可能有多張圖片，例如主圖、細節圖、情境圖。

### 5.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ImageId | BIGINT IDENTITY | PK | N | 圖片主鍵 |
| ProductId | BIGINT | FK | N | 商品 ID |
| SkuId | BIGINT | FK | Y | SKU ID，若圖片屬於特定規格才填 |
| ImageUrl | NVARCHAR(1000) |  | N | 圖片網址或路徑 |
| AltText | NVARCHAR(200) |  | Y | 替代文字 |
| IsMainImage | BIT |  | N | 是否主圖 |
| SortOrder | INT |  | N | 排序 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

---

## 6. ProductAttributes 商品屬性表

### 6.1 用途

`ProductAttributes` 用於定義商品規格屬性，例如顏色、尺寸、材質。

### 6.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| AttributeId | INT IDENTITY | PK | N | 屬性主鍵 |
| AttributeCode | NVARCHAR(50) | UK | N | 屬性代碼 |
| AttributeName | NVARCHAR(100) |  | N | 屬性名稱 |
| InputType | NVARCHAR(30) | CK | N | Text/Select/Number |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

### 6.3 範例

| AttributeCode | AttributeName | InputType |
|---|---|---|
| COLOR | 顏色 | Select |
| SIZE | 尺寸 | Select |
| MATERIAL | 材質 | Text |

---

## 7. ProductAttributeValues 商品屬性值表

### 7.1 用途

此表保存某個 SKU 的屬性值。

### 7.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| AttributeValueId | BIGINT IDENTITY | PK | N | 屬性值主鍵 |
| SkuId | BIGINT | FK | N | SKU ID |
| AttributeId | INT | FK | N | 屬性 ID |
| AttributeValue | NVARCHAR(200) |  | N | 屬性值 |

### 7.3 範例

| SkuNo | AttributeName | AttributeValue |
|---|---|---|
| SKU001 | 顏色 | 白色 |
| SKU001 | 尺寸 | M |
| SKU002 | 顏色 | 白色 |
| SKU002 | 尺寸 | L |

---

## 8. 商品模組關聯

```text
ProductCategories 1 ── N Products
Products 1 ── N ProductSkus
Products 1 ── N ProductImages
ProductSkus 1 ── N ProductImages
ProductSkus 1 ── N ProductAttributeValues
ProductAttributes 1 ── N ProductAttributeValues
```

---

## 9. 商品後台功能對應

| 後台功能 | 主要資料表 |
|---|---|
| 商品分類管理 | ProductCategories |
| 商品新增/修改 | Products, ProductSkus, ProductImages |
| SKU 管理 | ProductSkus, ProductAttributeValues |
| 商品上下架 | Products.Status, ProductSkus.Status |
| 商品圖片排序 | ProductImages.SortOrder |
| 商品搜尋 | Products.ProductName, ProductSkus.SkuNo, Barcode |

---

## 10. 商品模組總結

商品模組的設計重點在於「商品主檔」與「SKU」的分離。商品主檔負責描述商品，SKU 負責實際販售。這樣可以支援多規格、多價格、多庫存與多圖片的電商需求。
