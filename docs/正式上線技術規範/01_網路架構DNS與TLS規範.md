# 01｜網路架構、DNS 與 TLS 規範

## 1. 本章目的

電商網站正式上線後，使用者會透過網域進入前端，再由前端呼叫後端 API。網路架構要能保護傳輸安全、隔離內部資源，並讓服務穩定對外。

---

## 2. 建議網路拓樸

```text
使用者瀏覽器
    ↓ HTTPS
CDN / WAF
    ↓ HTTPS
Storefront App / Admin App
    ↓ HTTPS API
反向代理 / Load Balancer
    ↓
ASP.NET Core Web API
    ↓ 私有網路
MSSQL Database
```

初學者可以先理解：

```text
CDN：讓前端靜態檔案更快
WAF：擋常見 Web 攻擊
Load Balancer：分流請求
反向代理：幫後端統一入口
私有網路：資料庫不要直接暴露到網際網路
```

---

## 3. DNS 規劃

建議網域：

```text
www.example.com       前台網站
admin.example.com     後台管理
api.example.com       後端 API
```

DNS 檢查：

```text
□ A / CNAME 記錄正確
□ www 與裸網域轉址規劃完成
□ admin 後台不與前台混在同一入口
□ api 網域只提供 API
□ Storefront 與 Admin 可以分開設定快取、CORS 與安全政策
□ DNS TTL 設定合理
```

---

## 4. TLS / HTTPS 規範

正式環境必須使用 HTTPS。

檢查項目：

```text
□ TLS 憑證有效
□ 憑證自動更新機制已確認
□ HTTP 自動轉 HTTPS
□ 不允許舊版 TLS
□ API、前台、後台都使用 HTTPS
□ Cookie 若使用，需設定 Secure
```

---

## 5. HSTS 規劃

HSTS 可以要求瀏覽器之後只用 HTTPS 連線。

建議流程：

```text
1. 先確認全站 HTTPS 正常
2. Staging 測試 HSTS
3. Production 先使用較短 max-age
4. 確認無問題後再拉長時間
5. 慎重評估是否加入 preload
```

注意：HSTS 設錯可能導致使用者一段時間內無法用 HTTP 回退，所以要先測試。

---

## 6. 防火牆與連線限制

```text
□ MSSQL 不直接對外開放
□ 後端 API 只開必要 Port
□ 管理後台可限制 IP 或加強驗證
□ SSH / RDP 不開放給全網
□ 金流與物流 Webhook 來源可驗證
```

---

## 7. 本章總結

正式上線時，資料庫應在內部網路，外部使用者只透過 HTTPS 存取前端與 API。DNS、TLS、HSTS、防火牆與反向代理都要納入規劃，才算具備基本網路安全。
