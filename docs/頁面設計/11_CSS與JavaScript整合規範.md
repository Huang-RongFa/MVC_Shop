# 11｜CSS 與 JavaScript 整合規範

## 1. CSS 整合原則

DayNight 版型的 CSS 可以作為共用設計基礎。

建議：

```text
□ 保留 CSS Variables
□ 保留 Snow / Carbon 主題模式
□ 保留 Card、Button、Form、Table、Badge、Grid 樣式
□ 新增前台商品卡片與購物車樣式時另開 storefront.css
□ 新增後台管理表格與操作樣式時另開 admin.css
```

避免：

```text
□ 每個 cshtml 都寫大量 style
□ 任意覆蓋全域 class 導致前後台互相影響
□ 使用難以維護的 inline style
□ 不命名就新增一堆一次性 class
```

---

## 2. 主題切換

目前 JS 使用：

```text
localStorage daynight-theme
snow / carbon
```

這可以保留作為 UI 偏好設定。

注意：

```text
□ localStorage 只能存 UI 偏好
□ 不可存 Token
□ 不可存使用者權限
□ 不可存敏感個資
```

---

## 3. JavaScript 整合原則

可以保留：

```text
□ initTheme
□ setTheme
□ updateThemeButtons
□ mobile menu toggle
□ date range button active 狀態
□ settings toggle UI 狀態
```

需要調整：

```text
□ Kanban 拖曳若要保存狀態，必須呼叫後端 API
□ 日期範圍切換若要更新數據，必須呼叫後端或重新送出查詢
□ Inbox 內容不可用未清理 HTML 寫入 innerHTML
□ console.log 不應輸出正式敏感資料
```

---

## 4. XSS 注意事項

禁止：

```text
□ 將使用者輸入直接放入 innerHTML
□ 使用 Html.Raw 輸出未清理資料
□ 允許後台輸入任意 HTML 後直接顯示在前台
```

可以：

```text
□ 使用 Razor 預設 HTML Encoding
□ 使用 textContent 顯示純文字
□ 若需要富文字，必須有白名單清理機制
```

---

## 5. 表單安全

所有寫入型表單都要：

```text
□ 使用 POST
□ 加入 Anti-forgery Token
□ 後端加 [ValidateAntiForgeryToken]
□ 後端再次驗證輸入
□ 不相信前端傳來的價格、權限、角色
```

---

## 6. 正式上線前端資源

```text
□ CSS / JS 使用 asp-append-version
□ Bootstrap 固定版本
□ 若使用 CDN，評估 SRI
□ Source Map 是否公開需評估
□ 移除測試 console.log
□ 圖片最佳化
□ RWD 測試
```

---

## 7. 本章總結

CSS 可以共用設計語言，JS 可以保留主題與互動效果，但任何資料異動、權限、價格、庫存、訂單狀態都必須由後端決定。前端不能成為安全邊界。
