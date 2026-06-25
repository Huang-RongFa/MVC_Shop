# MVC_Shop 目前改善計畫

> 適用專案：`MVC_Shop`  
> 建議放置位置：`docs/CURRENT_IMPROVEMENT_PLAN.md`  
> 用途：作為目前專案優先改善清單，可提供給 Codex / Agent / 開發者逐項執行。  
> 狀態：依據目前專案靜態檢查與前一份診斷文件整理，尚未經 `dotnet build` / `dotnet test` 實際驗證。

---

## 1. 結論

目前專案的 MVC 架構方向正確，已具備前台、後台、登入、Service、Repository、DbContext 與 ViewModel 雛形；但距離正式電商系統仍需要優先補強以下項目：

1. **後台授權邊界不足**：一般會員登入後不應有機會進入 `/admin`。
2. **專案交付內容不乾淨**：ZIP / Repo 不應包含 `bin`、`obj`、Data Protection Keys、暫存專案與明文連線字串。
3. **docs 路徑與檔名需修正**：目前部分中文目錄曾被轉碼，會影響 README、CONTEXT、AGENTS 與 Agent 讀取。
4. **商品、購物車、結帳、訂單尚未正式資料化**：目前仍有展示資料、假資料與 TODO。
5. **缺少 Solution、測試專案、Migration 與正式驗收流程**。

建議先做 **安全與專案清理**，再做 **商品資料正式化**，最後再推進 **購物車、結帳、訂單、後台管理與部署流程**。

---

## 2. 改善優先順序

| 優先級 | 類型 | 改善項目 | 目的 |
|---|---|---|---|
| P0 | 安全 | 修正前後台登入與授權邊界 | 防止一般會員誤入後台 |
| P0 | 安全 | 清理敏感檔與不該交付的檔案 | 避免密碼、Key、暫存檔外洩 |
| P0 | 文件 | 修正 docs 目錄名稱與 README / CONTEXT / AGENTS 路徑 | 讓人與 Agent 都能正確讀取文件 |
| P1 | 架構 | 建立 `.sln` 與測試專案 | 後續開發可驗證、不靠人工猜測 |
| P1 | 資料庫 | 建立 Migration / Seed / DB 驗證流程 | 確保 Entity 與資料庫一致 |
| P1 | 商品 | 建立 ProductService / ProductRepository | 讓前台商品資料改由資料庫提供 |
| P1 | 購物車 | 正式化 CartService / CouponService | 讓價格、優惠、庫存由後端計算 |
| P1 | 訂單 | 建立 CheckoutService / OrderService | 建立正式下單交易流程 |
| P2 | 後台 | 商品、庫存、訂單、優惠、權限管理正式化 | 後台從切版變成可營運系統 |
| P2 | 上線 | CI、Health Check、Logging、Secret 管理 | 準備正式部署 |

---

## 3. P0：立即改善項目

### 3.1 修正後台授權邊界

#### 問題

目前前台會員與後台管理員可能共用 Cookie Authentication Scheme，且後台 Controller 若只使用 `[Authorize]`，代表只要「已登入」就可能被判定通過。

這對電商後台是高風險問題，因為後台會包含：

- 商品管理
- 庫存管理
- 訂單管理
- 付款與退款管理
- 會員資料
- 營收與分析資料
- 權限與稽核資料

#### 短期做法

先加入 `AdminOnly` Policy，至少確保一般 `Customer` 無法進入 `/admin`。

建議規則：

```csharp
options.AddPolicy("AdminOnly", policy =>
{
    policy.RequireAuthenticatedUser();
    policy.RequireClaim("UserType", "Admin", "Staff");
});
```

後台 Controller 必須使用：

```csharp
[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
}
```

若有 `/api/admin/*`，也必須套用相同 Policy。

#### 正式做法

建議拆成兩組登入 Cookie：

| 區域 | Cookie Scheme | 用途 |
|---|---|---|
| 前台 | `StorefrontCookie` | 一般會員登入、購物車、結帳、訂單查詢 |
| 後台 | `AdminCookie` | 管理員、客服、倉儲、營運人員登入 |

正式化後：

- 前台登入不能進後台。
- 後台登入不等於前台會員登入。
- 後台登出不影響前台會員登入狀態，反之亦然。
- 後台每個功能要依 Permission 控制。

#### 驗收標準

- [ ] Customer 登入後進入 `/admin` 會被拒絕或導向後台登入頁。
- [ ] 未登入使用者進入 `/admin` 會被導向後台登入頁。
- [ ] Admin 登入後可進入後台首頁。
- [ ] Staff 沒有指定 Permission 時，不能執行敏感操作。
- [ ] 後台 API 不只靠前端隱藏按鈕，後端也有授權檢查。

---

### 3.2 清理敏感檔與不該交付的檔案

#### 問題

目前 ZIP 中曾觀察到以下不應交付或不應進 Git 的內容：

```text
MyMVC/bin/
MyMVC/obj/
_tmp_password_reset/
MyMVC/App_Data/DataProtectionKeys/
appsettings.Development.json
```

其中：

- `bin` / `obj` 是編譯產物，不應提交。
- `_tmp_password_reset` 是暫存專案，不應留在正式專案。
- `DataProtectionKeys` 屬於敏感金鑰，不應放入分享檔。
- `appsettings.Development.json` 若含明文帳密，不應進公開版本。

#### 建議 `.gitignore` 補強

```gitignore
# Build output
bin/
obj/
.vs/

# User / IDE files
*.user
*.suo
*.rsuser

# Local secrets
appsettings.Development.json
appsettings.Local.json
*.secrets.json

# Data Protection Keys
**/App_Data/DataProtectionKeys/

# Temporary projects
_tmp*/
_tmp_password_reset/

# Logs
logs/
*.log
```

#### 建議清理指令 PowerShell

```powershell
cd E:\MVC_Shop

Remove-Item .\MyMVC\bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item .\MyMVC\obj -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item .\_tmp_password_reset -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item .\MyMVC\App_Data\DataProtectionKeys -Recurse -Force -ErrorAction SilentlyContinue
```

#### 重要提醒

若 `appsettings.Development.json` 曾經包含正式或可連線資料庫帳密，請視為已外洩，建議：

- [ ] 更換該資料庫帳號密碼。
- [ ] 停用舊帳號或降低權限。
- [ ] 改用 User Secrets 或環境變數。
- [ ] 不再把本機開發連線字串放進 ZIP。

#### 驗收標準

- [ ] 專案 ZIP 不含 `bin`、`obj`。
- [ ] 專案 ZIP 不含 `_tmp_password_reset`。
- [ ] 專案 ZIP 不含 `DataProtectionKeys`。
- [ ] 專案 ZIP 不含明文資料庫帳密。
- [ ] `.gitignore` 已補上上述規則。

---

### 3.3 修正 docs 目錄與檔名

#### 問題

目前部分 docs 中文路徑曾可能在 Windows / Git / ZIP 流程中被轉成 Unicode escape 形式，尤其是頁面設計與正式上線技術規範目錄。

這會造成：

- README.md 指到不存在的路徑。
- CONTEXT.md 與 AGENTS.md 讀取文件失敗。
- Codex / Agent 無法準確理解專案規範。
- Windows / Git / ZIP 解壓後路徑不一致。

#### 建議改成 ASCII 路徑

```text
docs/
├─ adr/
├─ MSSQL/
├─ MVC/
├─ page-design/
├─ production-readiness/
└─ CURRENT_IMPROVEMENT_PLAN.md
```

#### 需要同步更新的文件

- [x] `README.md`
- [x] `CONTEXT.md`
- [x] `AGENTS.md`
- [x] docs 內互相連結的 Markdown

#### 建議命名原則

| 類型 | 建議命名 |
|---|---|
| 頁面設計 | `docs/page-design/` |
| 正式上線技術規範 | `docs/production-readiness/` |
| MVC 架構文件 | `docs/MVC/` |
| 資料庫文件 | `docs/MSSQL/` |
| 架構決策紀錄 | `docs/adr/` |

#### 驗收標準

- [x] README 指向的 docs 路徑都存在。
- [x] CONTEXT 指向的 docs 路徑都存在。
- [x] AGENTS 指向的 docs 路徑都存在。
- [x] Windows、Git、ZIP 解壓後不再出現轉碼目錄。
- [x] Agent 能依文件順序讀取專案規範。

---

## 4. P1：近期核心改善項目

### 4.1 建立 Solution 與測試專案

#### 問題

目前專案若只有單一 `.csproj`，後續加入測試、CI、分層專案時會比較難管理。

#### 建議目標結構

```text
MVC_Shop/
├─ MVC_Shop.sln
├─ MyMVC/
│  └─ MyWeb.csproj
├─ tests/
│  ├─ MyWeb.UnitTests/
│  └─ MyWeb.IntegrationTests/
├─ docs/
└─ README.md
```

#### 建議指令

```powershell
cd E:\MVC_Shop

dotnet new sln -n MVC_Shop
dotnet sln add .\MyMVC\MyWeb.csproj

dotnet new xunit -n MyWeb.UnitTests -o .\tests\MyWeb.UnitTests
dotnet sln add .\tests\MyWeb.UnitTests\MyWeb.UnitTests.csproj
dotnet add .\tests\MyWeb.UnitTests\MyWeb.UnitTests.csproj reference .\MyMVC\MyWeb.csproj

dotnet new xunit -n MyWeb.IntegrationTests -o .\tests\MyWeb.IntegrationTests
dotnet sln add .\tests\MyWeb.IntegrationTests\MyWeb.IntegrationTests.csproj
dotnet add .\tests\MyWeb.IntegrationTests\MyWeb.IntegrationTests.csproj reference .\MyMVC\MyWeb.csproj
```

#### 優先測試項目

- [ ] Customer 不可進入 `/admin`。
- [ ] Admin 可進入 `/admin`。
- [ ] Cart 加入商品時會檢查商品是否存在。
- [ ] Cart 修改數量時不可超過庫存。
- [ ] Coupon 不可由前端自行決定折扣金額。
- [ ] Order 建立時會重新驗價。
- [ ] Order 建立失敗時不可扣庫存或留下半套資料。

---

### 4.2 建立 Migration / Seed / DB 驗證流程

#### 問題

目前雖然已有 Entity 與 DbContext，但若沒有 Migration 或 DB-first 驗證流程，未來資料庫結構很容易與程式模型不一致。

#### 需先決定

| 方案 | 說明 | 建議 |
|---|---|---|
| EF Code First | 以 Entity / DbContext 為主產生 Migration | 適合目前 MVC 專案快速演進 |
| DB First | 以 SQL Script 為主，再讓程式配合資料庫 | 適合資料庫設計已完全定稿的情況 |

目前建議：**先採 EF Code First，但保留 docs/MSSQL 作為資料庫設計文件與人工審核依據。**

#### 應建立的 Seed 資料

- [ ] Admin 帳號
- [ ] Staff 測試帳號
- [ ] Customer 測試帳號
- [ ] 商品分類
- [ ] 商品
- [ ] SKU
- [ ] 庫存
- [ ] 優惠券

#### 驗收標準

- [ ] 本機可用指令建立資料庫。
- [ ] Migration 可正常套用。
- [ ] Seed 後可以登入 Admin。
- [ ] Seed 後前台可以看到商品。
- [ ] Entity、DbContext、SQL 文件不互相矛盾。

---

### 4.3 商品資料正式化

#### 問題

目前前台商品頁、首頁商品區塊、部分後台數據仍可能使用 static 假資料或 ViewModel 展示資料。

#### 改善目標

建立正式商品查詢流程：

```text
Controller
  -> ProductService / ProductQueryService
    -> ProductRepository
      -> AppDbContext
        -> Products / ProductVariants / Inventory / Categories
```

#### 建議新增或整理的服務

```text
Application/Services/Catalog/
├─ ProductService.cs
├─ ProductQueryService.cs
├─ CategoryService.cs
└─ SkuService.cs
```

```text
Infrastructure/Repositories/Catalog/
├─ ProductRepository.cs
├─ CategoryRepository.cs
└─ SkuRepository.cs
```

#### 首批改造頁面

- [ ] 首頁商品區塊
- [ ] 商品列表頁
- [ ] 商品詳細頁
- [ ] 分類頁
- [ ] 搜尋頁

#### 查詢基本需求

- [ ] 分頁
- [ ] 關鍵字搜尋
- [ ] 分類篩選
- [ ] 價格排序
- [ ] 上架 / 下架狀態
- [ ] 庫存狀態
- [ ] SKU 規格顯示

#### 驗收標準

- [ ] 前台商品資料不再使用 static 假資料。
- [ ] 下架商品不顯示在前台。
- [ ] 無庫存商品可顯示，但不可購買或需標示售完。
- [ ] 商品價格由後端 Service 提供。
- [ ] 商品圖片路徑由資料庫或正式設定取得。

---

### 4.4 購物車與優惠正式化

#### 問題

購物車流程已有基礎，但優惠券、價格計算、庫存限制仍需要正式化，避免前端改數字或優惠規則寫死。

#### 改善目標

購物車金額必須由後端重新計算：

```text
CartController
  -> CartService
    -> PricingService
    -> CouponService
    -> InventoryService
```

#### 需要拆出的服務

- [ ] `PricingService`
- [ ] `CouponService`
- [ ] `PromotionService`
- [ ] `InventoryAvailabilityService`

#### 購物車規則

- [ ] 不信任前端傳來的商品價格。
- [ ] 不信任前端傳來的折扣金額。
- [ ] 修改數量時要檢查庫存上限。
- [ ] 刪除商品要記錄正常狀態，不要造成資料殘留。
- [ ] 優惠券要檢查有效期限、使用次數、最低金額、適用商品或分類。

#### 驗收標準

- [ ] 購物車小計由後端計算。
- [ ] 優惠券折扣由後端計算。
- [ ] 商品價格變動後，購物車顯示會重新依最新規則計算。
- [ ] 商品下架或無庫存時，購物車會提示使用者。
- [ ] Cart Service 有單元測試。

---

### 4.5 結帳與訂單交易正式化

#### 問題

目前結帳與訂單頁面若仍以展示資料為主，尚不能視為正式電商交易流程。

#### 改善目標

建立正式交易流程：

```text
CheckoutController
  -> CheckoutService
    -> CartService
    -> PricingService
    -> CouponService
    -> InventoryService
    -> OrderService
      -> OrderRepository
      -> InventoryReservationRepository
```

#### 下單時必須重新驗證

- [ ] 會員身分
- [ ] 商品是否上架
- [ ] SKU 是否存在
- [ ] 價格是否正確
- [ ] 優惠券是否仍有效
- [ ] 庫存是否足夠
- [ ] 收件資料是否完整
- [ ] 運送方式是否可用
- [ ] 付款方式是否可用

#### 交易一致性要求

下單時應在同一個 Transaction 內完成：

- [ ] 建立訂單主檔
- [ ] 建立訂單明細
- [ ] 建立金額快照
- [ ] 建立優惠快照
- [ ] 建立庫存保留
- [ ] 建立訂單狀態歷史
- [ ] 清空或鎖定購物車

#### 驗收標準

- [ ] 下單成功後可查到訂單主檔與明細。
- [ ] 下單成功後庫存保留正確。
- [ ] 下單失敗時不會留下半套訂單。
- [ ] 訂單金額不受前端竄改影響。
- [ ] 訂單狀態歷史完整紀錄。

---

## 5. P2：後續改善項目

### 5.1 後台管理功能正式化

#### 優先順序

| 順序 | 後台模組 | 原因 |
|---|---|---|
| 1 | 商品管理 | 影響前台商品顯示與銷售 |
| 2 | 庫存管理 | 影響可購買數量與出貨 |
| 3 | 訂單管理 | 電商營運核心 |
| 4 | 付款管理 | 影響對帳與交易狀態 |
| 5 | 出貨管理 | 影響履約流程 |
| 6 | 退款管理 | 涉及金額與稽核 |
| 7 | 優惠管理 | 影響價格與活動 |
| 8 | 權限管理 | 控制後台操作邊界 |
| 9 | 稽核紀錄 | 正式系統必備 |
| 10 | 數據分析 | 等資料流程穩定後再做 |

#### 每個後台模組都應具備

- [ ] 查詢
- [ ] 分頁
- [ ] 新增
- [ ] 編輯
- [ ] 停用或軟刪除
- [ ] 權限檢查
- [ ] 操作稽核
- [ ] 錯誤處理
- [ ] ViewModel 驗證
- [ ] Service 層商業規則

---

### 5.2 API 與 Vue 前後台分離

#### 建議原則

目前不建議立刻全面改成 Vue Storefront App / Vue Admin App。

原因：

- 商品、購物車、結帳、訂單 API 尚未穩定。
- 後台權限與交易流程尚未完成。
- 太早拆前端會讓問題從 MVC 變成 MVC + API + Vue 三邊同步修改。

#### 建議時機

等以下項目完成後再拆：

- [ ] `/api/storefront/products`
- [ ] `/api/storefront/cart`
- [ ] `/api/storefront/checkout`
- [ ] `/api/storefront/orders`
- [ ] `/api/admin/products`
- [ ] `/api/admin/inventory`
- [ ] `/api/admin/orders`
- [ ] `/api/admin/refunds`
- [ ] Admin Permission Policy

---

### 5.3 正式上線準備

#### 必備項目

- [ ] 環境分離：Development / Staging / Production
- [ ] Secret 管理：User Secrets / Environment Variables / Secret Manager
- [ ] Logging：結構化 Log
- [ ] Error Handling：正式錯誤頁與例外紀錄
- [ ] Health Check：資料庫、外部服務狀態
- [ ] CI：Build + Test
- [ ] CD：部署流程與回滾機制
- [ ] Backup：資料庫備份與還原演練
- [ ] CSP：Content Security Policy
- [ ] HTTPS：正式憑證與 HSTS
- [ ] Rate Limiting：登入與敏感 API 防暴力嘗試
- [ ] Audit Log：後台敏感操作紀錄

---

## 6. 建議工作單

以下可直接拆成 GitHub Issues 或 Codex 任務。

### SEC-001：修正後台授權邊界

#### 任務內容

- [x] 檢查 `Program.cs` 的 Authentication / Authorization 設定。
- [x] 新增 `AdminOnly` Policy。
- [x] 將 `AdminController` 改成 `[Authorize(Policy = "AdminOnly")]`。
- [x] 將所有 `/api/admin/*` 加上後台授權。
- [x] 檢查前台 Customer 是否無法進後台。

#### 完成標準

- [x] Customer 登入後不能進 `/admin`。
- [x] Admin 登入後可以進 `/admin`。
- [x] 未登入者會被導向正確登入頁。

---

### SEC-002：清理敏感檔與交付內容

#### 任務內容

- [x] 刪除 `bin`、`obj`。
- [x] 刪除 `_tmp_password_reset`。
- [x] 刪除 `App_Data/DataProtectionKeys`。
- [x] 檢查 `appsettings.Development.json` 是否含明文密碼。
- [x] 補強 `.gitignore`。

#### 完成標準

- [x] 交付清單不含敏感檔。
- [x] 交付清單不含 build 產物。
- [x] 專案仍可正常 build。

---

### DOC-001：修正 docs 路徑

#### 任務內容

- [x] 將轉碼後的頁面設計目錄改成 `page-design`。
- [x] 將轉碼後的正式上線技術規範目錄改成 `production-readiness`。
- [x] 更新 `README.md`。
- [x] 更新 `CONTEXT.md`。
- [x] 更新 `AGENTS.md`。
- [x] 檢查 Markdown 連結。

#### 完成標準

- [x] docs 內沒有轉碼目錄。
- [x] README / CONTEXT / AGENTS 指向的文件都存在。
- [x] Agent 可依文件正確理解專案。

---

### TEST-001：建立 Solution 與測試專案

#### 任務內容

- [ ] 建立 `MVC_Shop.sln`。
- [ ] 加入 `MyMVC/MyWeb.csproj`。
- [ ] 建立 `tests/MyWeb.UnitTests`。
- [ ] 建立 `tests/MyWeb.IntegrationTests`。
- [ ] 加入第一批 Auth / Cart 測試。

#### 完成標準

- [ ] `dotnet build` 通過。
- [ ] `dotnet test` 通過。
- [ ] 至少有 Auth 權限測試與 Cart 基礎測試。

---

### DB-001：建立資料庫版本管理流程

#### 任務內容

- [ ] 確認採用 EF Code First 或 DB First。
- [ ] 若採 EF Code First，建立第一版 Migration。
- [ ] 建立 Seed 資料。
- [ ] 補上資料庫初始化文件。

#### 完成標準

- [ ] 本機可以重建資料庫。
- [ ] Seed 後可登入 Admin。
- [ ] Seed 後前台有商品資料。

---

### CAT-001：商品資料正式化

#### 任務內容

- [ ] 建立 `ProductService`。
- [ ] 建立 `ProductQueryService`。
- [ ] 建立 `ProductRepository`。
- [ ] 首頁商品區塊改從 DB 查詢。
- [ ] 商品列表頁改從 DB 查詢。
- [ ] 商品詳細頁改從 DB 查詢。

#### 完成標準

- [ ] 前台商品不再使用 static 假資料。
- [ ] 下架商品不顯示。
- [ ] 商品價格與庫存由後端提供。

---

### CART-001：購物車與優惠正式化

#### 任務內容

- [ ] 建立 `PricingService`。
- [ ] 建立 `CouponService`。
- [ ] 將優惠券邏輯從 static / hard-code 移出。
- [ ] Cart 每次顯示都重新計算金額。
- [ ] 修改數量時檢查庫存。

#### 完成標準

- [ ] 前端不可竄改價格。
- [ ] 前端不可竄改折扣。
- [ ] Cart 測試通過。

---

### ORDER-001：建立正式下單流程

#### 任務內容

- [ ] 建立 `CheckoutService`。
- [ ] 建立 `OrderService`。
- [ ] 建立訂單主檔與明細。
- [ ] 建立庫存保留。
- [ ] 建立訂單狀態歷史。
- [ ] 下單流程加入 Transaction。

#### 完成標準

- [ ] 下單成功會產生完整訂單。
- [ ] 下單失敗不會留下半套資料。
- [ ] 訂單金額由後端重新驗算。

---

## 7. Codex / Agent 執行提示詞

### 7.1 第一階段提示詞：安全與專案清理

```text
請先閱讀 README.md、CONTEXT.md、AGENTS.md，以及 docs/CURRENT_IMPROVEMENT_PLAN.md。

本次任務只處理 P0 項目，不要重構大型架構，也不要新增不必要頁面。

請完成：
1. 修正後台授權邊界，確保 Customer 無法進入 /admin。
2. 新增或調整 AdminOnly Policy。
3. 清理不該進 Git / ZIP 的 bin、obj、DataProtectionKeys、_tmp_password_reset。
4. 補強 .gitignore。
5. 修正 docs 目錄轉碼問題，並同步更新 README.md、CONTEXT.md、AGENTS.md。

完成後請執行 dotnet build；如果無法執行，請明確說明原因。
請列出修改過的檔案與驗收方式。
```

---

### 7.2 第二階段提示詞：商品資料正式化

```text
請先閱讀 README.md、CONTEXT.md、AGENTS.md，以及 docs/CURRENT_IMPROVEMENT_PLAN.md。

本次任務處理 CAT-001：商品資料正式化。

請完成：
1. 建立 ProductService / ProductQueryService。
2. 建立 ProductRepository 或補齊既有 Repository。
3. 將首頁商品區塊、商品列表頁、商品詳細頁改為從資料庫查詢。
4. 不要讓 Controller 直接操作 DbContext。
5. 不要讓 Entity 直接回傳到 View，請使用 ViewModel / DTO。
6. 加入必要的查詢條件：分頁、分類、上架狀態、庫存狀態。

完成後請執行 dotnet build 與相關測試。
請列出修改過的檔案與驗收方式。
```

---

### 7.3 第三階段提示詞：購物車與訂單正式化

```text
請先閱讀 README.md、CONTEXT.md、AGENTS.md，以及 docs/CURRENT_IMPROVEMENT_PLAN.md。

本次任務處理 CART-001 與 ORDER-001。

請完成：
1. 建立 PricingService / CouponService。
2. 購物車金額、優惠、庫存限制都由後端重新計算。
3. 建立 CheckoutService / OrderService。
4. 下單時重新驗證商品、價格、優惠券、庫存、會員與收件資料。
5. 下單流程必須使用 Transaction。
6. 訂單建立成功後要有訂單主檔、訂單明細、金額快照、優惠快照、庫存保留與狀態歷史。

請避免 Controller 直接操作 DbContext。
請補上核心單元測試或整合測試。
完成後請列出修改過的檔案與驗收方式。
```

---

## 8. 總驗收標準

完成本文件主要改善後，專案至少要達到以下狀態：

### 8.1 安全

- [ ] Customer 不能進後台。
- [ ] Admin / Staff 權限分流清楚。
- [ ] 後台 API 有授權保護。
- [ ] ZIP / Repo 不含敏感檔。
- [ ] 明文密碼不進版控。

### 8.2 架構

- [ ] Controller 不直接操作 DbContext。
- [ ] Service 負責商業流程。
- [ ] Repository 負責資料存取。
- [ ] ViewModel / DTO 不直接暴露 Entity。
- [ ] 前台與後台 Layout 與權限邊界清楚。

### 8.3 資料

- [ ] 商品資料來自資料庫。
- [ ] 購物車金額由後端計算。
- [ ] 優惠券由後端驗證。
- [ ] 訂單金額有快照。
- [ ] 庫存有保留與扣減規則。

### 8.4 測試

- [ ] `dotnet build` 通過。
- [ ] `dotnet test` 通過。
- [ ] Auth / Authorization 有測試。
- [ ] Cart / Coupon 有測試。
- [ ] Checkout / Order 有測試。

### 8.5 文件

- [x] README.md、CONTEXT.md、AGENTS.md 路徑一致。
- [x] docs 目錄不再有亂碼或轉碼目錄。
- [x] 主要設計文件可被 Agent 正確讀取。
- [x] 本文件已放入 `docs/CURRENT_IMPROVEMENT_PLAN.md`。

---

## 9. 建議執行順序

建議不要一次全做，請依序完成：

1. **SEC-001：修正後台授權邊界**
2. **SEC-002：清理敏感檔與交付內容**
3. **DOC-001：修正 docs 路徑**
4. **TEST-001：建立 Solution 與測試專案**
5. **DB-001：建立資料庫版本管理流程**
6. **CAT-001：商品資料正式化**
7. **CART-001：購物車與優惠正式化**
8. **ORDER-001：正式下單流程**
9. **ADMIN-001：後台管理模組正式化**
10. **OPS-001：正式上線準備**

---

## 10. 最後建議

目前最適合先做的是：

> **後台授權修正 + 敏感檔清理 + docs 路徑修復。**

這三項完成後，專案才比較適合交給 Codex / Agent 繼續做商品、購物車與訂單流程。  
若直接跳去做更多頁面，會讓畫面越來越多，但正式交易能力、安全邊界與資料一致性仍然不足。
