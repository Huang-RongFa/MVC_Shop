# ADR-0002｜共用 Users 資料表並以 UserType 區分身分

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`01_資料庫與EFCore正式對應.md`、`04_登入驗證Cookie權限與CSRF安全.md`、`06_RESTfulAPI前後台API與交易流程.md`

## 1. 背景與問題

系統同時有前台顧客、後台管理員與一般員工。若一開始拆成 `Customers`、`Admins`、`Staffs` 等多張登入資料表，會讓登入驗證、密碼雜湊、登入紀錄、權限與稽核重複設計。

但若只使用一張 `Users` 而沒有明確身分界線，也會造成前台會員誤入後台、後台權限混亂或授權判斷散落。

## 2. 決策

所有可登入帳號共用一張 `Users` 資料表，並用 `UserType` 區分主要身分：

```text
Customer：前台會員
Admin：系統管理員
Staff：後台一般員工
```

角色與權限另由下列表格管理：

```text
Roles
UserRoles
Permissions
RolePermissions
UserLoginLogs
```

## 3. 理由

- 登入驗證、密碼雜湊、Cookie、登入紀錄可以集中處理。
- `UserType` 負責粗粒度身分邊界，`Roles / Permissions` 負責細粒度授權。
- 前台與後台仍可透過不同 Controller、Route、Policy 與 Service 檢查分離。

## 4. 實作要求

```text
□ Storefront Login 僅允許 Customer 或被明確允許的身分
□ Admin Login 僅允許 Admin / Staff
□ 後台 API 必須檢查 Role 或 Permission
□ 前台 API 必須檢查資料擁有者
□ 登入成功與失敗皆寫入 UserLoginLogs
□ 不直接回傳 User Entity 給前端
□ 不回傳 PasswordHash、SecurityStamp、內部稽核欄位
```

## 5. 影響與取捨

| 面向 | 影響 |
|---|---|
| 優點 | 登入與權限集中，避免多套身分驗證邏輯 |
| 成本 | Service 需明確檢查 UserType、Role、Permission |
| 風險 | 若只檢查登入不檢查 UserType，可能造成前後台越權 |

## 6. 驗收檢查

```text
□ 前台會員不能登入後台
□ 後台人員不能用前台權限操作其他會員資料
□ 權限不足回傳 403
□ 未登入回傳 401
□ 登入紀錄可追蹤 IP、UserAgent、成功或失敗原因
```
