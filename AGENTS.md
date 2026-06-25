# AGENTS.md

## 開發語言

請使用繁體中文回覆，除非使用者指定其他語言。

## 專案規範讀取順序

在修改程式碼前，請優先閱讀：

1. CONTEXT.md
2. README.md
3. docs/adr/*
4. docs/MSSQL/*
5. docs/MVC/*
6. docs/production-readiness/*
7. docs/page-design/*

## 專案定位

本專案是完整交易型電商系統，不是教學型 CRUD。

核心架構：

- ASP.NET Core Web API 模組化單體
- 共用 MSSQL Database
- Storefront Vue App
- Admin Vue App
- Cookie Authentication
- Role / Permission Authorization

## 開發原則

請遵守：

- 小步修改
- 可測試
- 可回復
- 安全優先
- 不任意大規模重構
- 不任意新增套件
- 不任意修改資料庫結構

## 分層責任

Controller 只負責 Request / Response。

Service 負責商業流程、權限判斷、交易控制。

Repository 負責資料存取。

DbContext 負責 EF Core 與 MSSQL 連線。

Entity 不可直接回傳 API。

DTO / ViewModel 才能作為 API 或 View 的資料模型。

## mattpocock skills 使用方式

請採用 mattpocock/skills 的工程流程精神，但不要硬套 TypeScript 或 Node.js 慣例。

適用方式：

- 需求不清楚：使用 /grill-me 或 /grill-with-docs 精神
- 複雜商業規則：使用 /tdd 精神
- Bug 修正：使用 /diagnose 精神
- 架構調整：使用 /request-refactor-plan 精神
- 任務拆解：使用 /to-issues 精神

若需要引用這些流程，請以 CONTEXT.md 中的本專案規則為優先。

## 寫程式前必須先說明

請先輸出：

【目的說明】

【目前判斷】

【建議做法】

【會修改的檔案】

【風險與替代方案】

再提供程式碼。
---

# 頁面設計補充規則

## 頁面設計文件讀取

當任務涉及 Razor Views、HTML、CSS、JavaScript、版面配置、前台頁面、後台頁面、登入頁面或 DayNight 版型整合時，必須額外閱讀：

```text
docs/page-design/README_頁面設計總覽.md
docs/page-design/00_版型來源與整合原則.md
docs/page-design/01_前台與後台頁面邊界.md
docs/page-design/02_Views資料夾規劃.md
```

## 頁面設計核心規則

* 前台頁面可以讓未登入使用者瀏覽商品，但不能顯示後台數據分析。
* 登入後的前台會員才可以加入購物車、結帳、付款與查詢自己的訂單。
* 後台 Dashboard、Analytics、營收、訂單統計、庫存調整、退款與稽核只允許 Admin / Staff 依權限查看。
* DayNight HTML 版型只能作為視覺來源，不可直接保留假資料作為正式資料。
* `.cshtml` 不應直接使用 Entity，應使用 ViewModel。
* CSS / JS 應集中放在 `wwwroot/css` 與 `wwwroot/js`，不要散落在每個 View。
* 使用者輸入不可直接放入 `innerHTML` 或 `Html.Raw`。

## 修改頁面前必須說明

除了原本的目的、判斷、做法、修改檔案與風險之外，涉及頁面設計時還要說明：

```text
【頁面區域】前台 / 後台 / 帳號
【使用 Layout】_StorefrontLayout / _AdminLayout / _AuthLayout
【參考版型】index.html / login.html / analytics.html / projects.html / inbox.html / settings.html / about-templatemo.html
【登入需求】匿名可看 / 需登入 / 需後台權限
【資料來源】ViewModel / Service / API
```
