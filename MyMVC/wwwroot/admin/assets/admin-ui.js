function openAdminSidebar() {
    document.getElementById("adminSidebar")?.classList.add("open");
    document.getElementById("adminMobileBackdrop")?.classList.add("open");
    document.body.classList.add("admin-menu-open");
}

function closeAdminSidebar() {
    document.getElementById("adminSidebar")?.classList.remove("open");
    document.getElementById("adminMobileBackdrop")?.classList.remove("open");
    document.body.classList.remove("admin-menu-open");
}

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        closeAdminSidebar();
    }
});

const adminApi = (() => {
    const csrfEndpoint = "/api/security/csrf-token";

    function read(payload, camelName, pascalName) {
        return payload?.[camelName] ?? payload?.[pascalName];
    }

    function normalizeApiResponse(payload) {
        return {
            succeeded: read(payload, "succeeded", "Succeeded") ?? false,
            data: read(payload, "data", "Data") ?? null,
            error: read(payload, "error", "Error") ?? null
        };
    }

    function errorMessage(error, fallback) {
        return read(error, "message", "Message") ?? fallback;
    }

    async function requestCsrfToken() {
        const response = await fetch(csrfEndpoint, {
            method: "GET",
            credentials: "include"
        });

        const payload = normalizeApiResponse(await response.json());
        const data = payload.data;

        if (!response.ok || !payload.succeeded || !read(data, "token", "Token")) {
            throw new Error(errorMessage(payload.error, "無法取得安全驗證權杖。"));
        }

        return {
            headerName: read(data, "headerName", "HeaderName") ?? "X-CSRF-TOKEN",
            token: read(data, "token", "Token")
        };
    }

    async function json(url, options = {}) {
        const method = options.method ?? "GET";
        const headers = {
            "Accept": "application/json",
            ...(options.headers ?? {})
        };

        if (method !== "GET" && method !== "HEAD") {
            const csrf = await requestCsrfToken();
            headers["Content-Type"] = "application/json";
            headers[csrf.headerName] = csrf.token;
        }

        const response = await fetch(url, {
            method,
            credentials: "include",
            headers,
            body: options.body === undefined ? undefined : JSON.stringify(options.body)
        });

        const payload = normalizeApiResponse(await response.json());
        if (!response.ok || !payload.succeeded) {
            throw new Error(errorMessage(payload.error, "後台資料處理失敗。"));
        }

        return payload.data;
    }

    return {
        json,
        read
    };
})();

document.addEventListener("DOMContentLoaded", () => {
    initAdminUsersPage();
    initAdminRolesPage();
    initAdminAuditLogsPage();
    initAdminSettingsPage();
    initAdminProductsPage();
    initAdminProductEditPage();
    initAdminSkusPage();
    initAdminInventoryPage();
    initAdminOrdersPage();
});

function initAdminUsersPage() {
    const page = document.querySelector("[data-admin-users-page]");
    if (!page) {
        return;
    }

    const state = {
        page: 1,
        pageSize: 10
    };

    const body = page.querySelector("[data-admin-users-body]");
    const message = page.querySelector("[data-admin-users-message]");
    const metrics = document.querySelector("[data-admin-users-metrics]");
    const refreshButton = page.querySelector("[data-admin-users-refresh]");
    const pageSizeSelect = page.querySelector("[data-admin-users-page-size]");
    const pagination = page.querySelector("[data-admin-users-pagination]");

    refreshButton?.addEventListener("click", () => loadUsers());
    pageSizeSelect?.addEventListener("change", () => {
        state.page = 1;
        state.pageSize = Number(pageSizeSelect.value) || 10;
        void loadUsers();
    });

    void loadUsers();

    async function loadUsers(page = state.page) {
        state.page = page;
        showMessage(message, "載入中。", "neutral");

        try {
            const data = await adminApi.json(`/api/admin/system/users?page=${state.page}&pageSize=${state.pageSize}`);
            state.page = Number(adminApi.read(data, "page", "Page")) || state.page;
            state.pageSize = Number(adminApi.read(data, "pageSize", "PageSize")) || state.pageSize;
            renderUserMetrics(metrics, data);
            renderUsers(body, message, data, () => loadUsers(state.page));
            renderPagination(pagination, data, loadUsers);
            hideMessage(message);
        } catch (error) {
            showMessage(message, error.message, "danger");
        }
    }

    function renderUserMetrics(container, data) {
        if (!container) {
            return;
        }

        const users = getArray(data, "users", "Users");
        const activeCount = users.filter((user) => getText(user, "status", "Status") === "Active").length;
        const staffCount = users.filter((user) => getText(user, "userType", "UserType") !== "Customer").length;

        container.replaceChildren(
            statCard("使用者總數", String(adminApi.read(data, "totalCount", "TotalCount") ?? users.length), "Users", "neutral"),
            statCard("本頁啟用", String(activeCount), "Active", "success"),
            statCard("本頁後台帳號", String(staffCount), "Admin / Staff", "warning"),
            statCard("可指派角色", String(getArray(data, "roleOptions", "RoleOptions").length), "Roles", "neutral")
        );
    }
}

function initAdminRolesPage() {
    const page = document.querySelector("[data-admin-roles-page]");
    if (!page) {
        return;
    }

    const body = page.querySelector("[data-admin-roles-body]");
    const message = page.querySelector("[data-admin-roles-message]");
    const refreshButton = page.querySelector("[data-admin-roles-refresh]");

    refreshButton?.addEventListener("click", loadRoles);
    void loadRoles();

    async function loadRoles() {
        showMessage(message, "載入中。", "neutral");

        try {
            const data = await adminApi.json("/api/admin/system/roles");
            renderRoles(body, message, data, loadRoles);
            hideMessage(message);
        } catch (error) {
            showMessage(message, error.message, "danger");
        }
    }
}

function initAdminAuditLogsPage() {
    const page = document.querySelector("[data-admin-audit-page]");
    if (!page) {
        return;
    }

    const metrics = document.querySelector("[data-admin-audit-metrics]");
    const message = page.querySelector("[data-admin-audit-message]");
    const refreshButton = document.querySelector("[data-admin-audit-refresh]");
    const actionBody = page.querySelector("[data-admin-action-log-body]");
    const dataChangeBody = document.querySelector("[data-admin-data-change-body]");
    const systemErrorBody = document.querySelector("[data-admin-system-error-body]");

    refreshButton?.addEventListener("click", loadAuditTrail);
    void loadAuditTrail();

    async function loadAuditTrail() {
        showMessage(message, "載入中。", "neutral");

        try {
            const data = await adminApi.json("/api/admin/audit-logs");
            renderAuditMetrics(metrics, data);
            renderAdminActionLogs(actionBody, data);
            renderDataChangeLogs(dataChangeBody, data);
            renderSystemErrorLogs(systemErrorBody, data);
            hideMessage(message);
        } catch (error) {
            showMessage(message, error.message, "danger");
        }
    }
}

function initAdminSettingsPage() {
    const page = document.querySelector("[data-admin-settings-page]");
    if (!page) {
        return;
    }

    const metrics = document.querySelector("[data-admin-settings-metrics]");
    const message = page.querySelector("[data-admin-settings-message]");
    const refreshButton = document.querySelector("[data-admin-settings-refresh]");
    const sectionsContainer = page.querySelector("[data-admin-settings-sections]");

    refreshButton?.addEventListener("click", loadSettings);
    void loadSettings();

    async function loadSettings() {
        showMessage(message, "載入中。", "neutral");

        try {
            const data = await adminApi.json("/api/admin/settings");
            renderSettingsMetrics(metrics, data);
            renderSettingsSections(sectionsContainer, data);
            hideMessage(message);
        } catch (error) {
            showMessage(message, error.message, "danger");
        }
    }
}

function initAdminProductsPage() {
    initPagedAdminListPage({
        pageSelector: "[data-admin-products-page]",
        bodySelector: "[data-admin-products-body]",
        messageSelector: "[data-admin-products-message]",
        metricsSelector: "[data-admin-products-metrics]",
        refreshSelector: "[data-admin-products-refresh]",
        pageSizeSelector: "[data-admin-products-page-size]",
        paginationSelector: "[data-admin-products-pagination]",
        endpoint: "/api/admin/products",
        renderMetrics: renderProductMetrics,
        renderRows: renderProducts
    });
}

function initAdminProductEditPage() {
    const page = document.querySelector("[data-admin-product-edit-page]");
    if (!page) {
        return;
    }

    const query = new URLSearchParams(window.location.search);
    const initialProductId = Number(query.get("id"));
    const state = {
        productId: Number.isFinite(initialProductId) && initialProductId > 0 ? initialProductId : null,
        skuId: null,
        images: []
    };

    const form = page.querySelector("[data-admin-product-form]");
    const message = page.querySelector("[data-admin-product-edit-message]");
    const title = document.querySelector("[data-admin-product-edit-title]");
    const imageContainer = page.querySelector("[data-admin-product-images]");
    const addImageButton = page.querySelector("[data-admin-add-product-image]");
    const saveButton = page.querySelector("[data-admin-product-save]");

    addImageButton?.addEventListener("click", () => {
        syncImagesFromRows();
        state.images.push({
            imageId: null,
            imageUrl: "",
            altText: "",
            isMainImage: state.images.length === 0,
            sortOrder: state.images.length
        });
        renderProductImageRows();
    });

    form?.addEventListener("submit", async (event) => {
        event.preventDefault();
        await saveProduct();
    });

    void loadProductEditor();

    async function loadProductEditor() {
        showMessage(message, "載入中。", "neutral");

        try {
            const endpoint = state.productId
                ? `/api/admin/products/${state.productId}/edit`
                : "/api/admin/products/edit-options";
            const data = await adminApi.json(endpoint);
            populateProductForm(data);
            hideMessage(message);
        } catch (error) {
            showMessage(message, error.message, "danger");
        }
    }

    function populateProductForm(data) {
        const productId = adminApi.read(data, "productId", "ProductId");
        const productNo = getText(data, "productNo", "ProductNo");
        const sku = adminApi.read(data, "sku", "Sku") ?? {};
        const categoryOptions = getArray(data, "categoryOptions", "CategoryOptions");
        const statuses = getArray(data, "availableStatuses", "AvailableStatuses");
        const skuStatuses = getArray(data, "availableSkuStatuses", "AvailableSkuStatuses");

        state.productId = productId ? Number(productId) : null;
        state.skuId = adminApi.read(sku, "skuId", "SkuId") ?? null;
        state.images = getArray(data, "images", "Images").map((image, index) => ({
            imageId: adminApi.read(image, "imageId", "ImageId") ?? null,
            imageUrl: getText(image, "imageUrl", "ImageUrl"),
            altText: getText(image, "altText", "AltText"),
            isMainImage: Boolean(adminApi.read(image, "isMainImage", "IsMainImage")),
            sortOrder: Number(adminApi.read(image, "sortOrder", "SortOrder")) || index
        }));

        if (title) {
            title.textContent = state.productId
                ? `編輯商品 ${productNo || state.productId}`
                : "新增商品";
        }

        setFieldValue("productName", getText(data, "productName", "ProductName"));
        setFieldValue("brandName", getText(data, "brandName", "BrandName"));
        setFieldValue("shortDescription", getText(data, "shortDescription", "ShortDescription"));
        setFieldValue("fullDescription", getText(data, "fullDescription", "FullDescription"));
        setFieldChecked("isFeatured", Boolean(adminApi.read(data, "isFeatured", "IsFeatured")));
        fillSelect(
            field("categoryId"),
            categoryOptions,
            adminApi.read(data, "categoryId", "CategoryId"),
            (option) => adminApi.read(option, "categoryId", "CategoryId"),
            (option) => `${getText(option, "categoryName", "CategoryName")} (${getText(option, "categoryCode", "CategoryCode")})`);
        fillSimpleSelect(field("status"), statuses, getText(data, "status", "Status") || "Draft");

        setFieldValue("skuNo", getText(sku, "skuNo", "SkuNo"));
        setFieldValue("skuName", getText(sku, "skuName", "SkuName"));
        setFieldValue("specText", getText(sku, "specText", "SpecText"));
        setFieldValue("listPrice", numberOrEmpty(adminApi.read(sku, "listPrice", "ListPrice")));
        setFieldValue("salePrice", numberOrEmpty(adminApi.read(sku, "salePrice", "SalePrice")));
        setFieldValue("basePrice", numberOrEmpty(adminApi.read(sku, "basePrice", "BasePrice")));
        setFieldValue("size", getText(sku, "size", "Size"));
        setFieldValue("color", getText(sku, "color", "Color"));
        setFieldValue("capacity", getText(sku, "capacity", "Capacity"));
        fillSimpleSelect(field("skuStatus"), skuStatuses, getText(sku, "status", "Status") || "Active");

        const mainImage = state.images.find((image) => image.isMainImage) ?? state.images[0];
        setFieldValue("mainImageUrl", mainImage?.imageUrl ?? "");
        renderProductImageRows();
    }

    async function saveProduct() {
        if (!form?.reportValidity()) {
            return;
        }

        showMessage(message, "儲存中。", "neutral");
        if (saveButton) {
            saveButton.disabled = true;
        }

        try {
            const payload = collectProductPayload();
            const endpoint = state.productId
                ? `/api/admin/products/${state.productId}`
                : "/api/admin/products";
            const method = state.productId ? "PUT" : "POST";
            const result = await adminApi.json(endpoint, {
                method,
                body: payload
            });

            const savedProductId = Number(adminApi.read(result, "productId", "ProductId"));
            if (Number.isFinite(savedProductId) && savedProductId > 0) {
                state.productId = savedProductId;
                window.history.replaceState({}, "", `/admin/products/edit?id=${savedProductId}`);
            }

            showMessage(message, getText(result, "message", "Message") || "商品已儲存。", "success");
            await loadProductEditor();
        } catch (error) {
            showMessage(message, error.message, "danger");
        } finally {
            if (saveButton) {
                saveButton.disabled = false;
            }
        }
    }

    function collectProductPayload() {
        const images = Array.from(imageContainer?.querySelectorAll("[data-product-image-row]") ?? [])
            .map((row, index) => ({
                imageId: readNullableNumber(row.querySelector("[data-image-field='imageId']")?.value),
                imageUrl: row.querySelector("[data-image-field='imageUrl']")?.value?.trim() ?? "",
                altText: row.querySelector("[data-image-field='altText']")?.value?.trim() || null,
                isMainImage: Boolean(row.querySelector("[data-image-field='isMainImage']")?.checked),
                sortOrder: readNumber(row.querySelector("[data-image-field='sortOrder']")?.value, index)
            }))
            .filter((image) => image.imageUrl.length > 0);

        return {
            categoryId: readNumber(field("categoryId")?.value, 0),
            productName: field("productName")?.value?.trim() ?? "",
            brandName: field("brandName")?.value?.trim() || null,
            shortDescription: field("shortDescription")?.value?.trim() || null,
            fullDescription: field("fullDescription")?.value?.trim() || null,
            status: field("status")?.value ?? "Draft",
            isFeatured: Boolean(field("isFeatured")?.checked),
            sku: {
                skuId: state.skuId,
                skuNo: field("skuNo")?.value?.trim() || null,
                skuName: field("skuName")?.value?.trim() ?? "",
                specText: field("specText")?.value?.trim() || null,
                listPrice: readNumber(field("listPrice")?.value, 0),
                salePrice: readNumber(field("salePrice")?.value, 0),
                basePrice: readNullableNumber(field("basePrice")?.value),
                size: field("size")?.value?.trim() || null,
                color: field("color")?.value?.trim() || null,
                capacity: field("capacity")?.value?.trim() || null,
                status: field("skuStatus")?.value ?? "Active"
            },
            mainImageUrl: field("mainImageUrl")?.value?.trim() || null,
            images,
            reason: field("reason")?.value?.trim() || null
        };
    }

    function renderProductImageRows() {
        if (!imageContainer) {
            return;
        }

        if (state.images.length === 0) {
            imageContainer.replaceChildren(emptyBlock("目前沒有其他商品圖片。"));
            return;
        }

        imageContainer.replaceChildren(...state.images.map(productImageRow));
    }

    function productImageRow(image, index) {
        const row = document.createElement("section");
        row.className = "admin-image-row";
        row.dataset.productImageRow = "true";

        const imageId = document.createElement("input");
        imageId.type = "hidden";
        imageId.value = image.imageId ?? "";
        imageId.dataset.imageField = "imageId";

        const url = imageInput("圖片路徑", "imageUrl", image.imageUrl, 1000);
        const alt = imageInput("替代文字", "altText", image.altText, 200);
        const sort = imageInput("排序", "sortOrder", String(image.sortOrder ?? index), 6, "number");

        const main = document.createElement("label");
        main.className = "admin-check-row";
        const mainInput = document.createElement("input");
        mainInput.type = "checkbox";
        mainInput.checked = image.isMainImage;
        mainInput.dataset.imageField = "isMainImage";
        mainInput.addEventListener("change", () => {
            if (mainInput.checked) {
                for (const checkbox of imageContainer?.querySelectorAll("[data-image-field='isMainImage']") ?? []) {
                    if (checkbox !== mainInput) {
                        checkbox.checked = false;
                    }
                }

                const mainUrl = row.querySelector("[data-image-field='imageUrl']")?.value?.trim();
                if (mainUrl) {
                    setFieldValue("mainImageUrl", mainUrl);
                }
            }
        });
        const mainText = document.createElement("span");
        mainText.textContent = "設為主圖";
        main.append(mainInput, mainText);

        const remove = button("移除", "admin-table-action");
        remove.addEventListener("click", () => {
            syncImagesFromRows();
            state.images.splice(index, 1);
            renderProductImageRows();
        });

        row.append(imageId, url, alt, sort, main, remove);
        return row;
    }

    function imageInput(labelText, fieldName, value, maxLength, type = "text") {
        const label = document.createElement("label");
        label.className = "admin-form-field";

        const span = document.createElement("span");
        span.textContent = labelText;

        const input = document.createElement("input");
        input.className = "admin-control";
        input.type = type;
        input.maxLength = maxLength;
        input.value = value ?? "";
        input.dataset.imageField = fieldName;

        label.append(span, input);
        return label;
    }

    function syncImagesFromRows() {
        if (!imageContainer) {
            return;
        }

        state.images = Array.from(imageContainer.querySelectorAll("[data-product-image-row]"))
            .map((row, index) => ({
                imageId: readNullableNumber(row.querySelector("[data-image-field='imageId']")?.value),
                imageUrl: row.querySelector("[data-image-field='imageUrl']")?.value?.trim() ?? "",
                altText: row.querySelector("[data-image-field='altText']")?.value?.trim() ?? "",
                isMainImage: Boolean(row.querySelector("[data-image-field='isMainImage']")?.checked),
                sortOrder: readNumber(row.querySelector("[data-image-field='sortOrder']")?.value, index)
            }));
    }

    function field(name) {
        return page.querySelector(`[data-field='${name}']`);
    }

    function setFieldValue(name, value) {
        const element = field(name);
        if (element) {
            element.value = value ?? "";
        }
    }

    function setFieldChecked(name, checked) {
        const element = field(name);
        if (element) {
            element.checked = checked;
        }
    }
}

function initAdminSkusPage() {
    initPagedAdminListPage({
        pageSelector: "[data-admin-skus-page]",
        bodySelector: "[data-admin-skus-body]",
        messageSelector: "[data-admin-skus-message]",
        metricsSelector: "[data-admin-skus-metrics]",
        refreshSelector: "[data-admin-skus-refresh]",
        pageSizeSelector: "[data-admin-skus-page-size]",
        paginationSelector: "[data-admin-skus-pagination]",
        endpoint: "/api/admin/products/skus",
        renderMetrics: renderSkuMetrics,
        renderRows: renderSkus
    });
}

function initAdminInventoryPage() {
    initPagedAdminListPage({
        pageSelector: "[data-admin-inventory-page]",
        bodySelector: "[data-admin-inventory-body]",
        messageSelector: "[data-admin-inventory-message]",
        metricsSelector: "[data-admin-inventory-metrics]",
        refreshSelector: "[data-admin-inventory-refresh]",
        pageSizeSelector: "[data-admin-inventory-page-size]",
        paginationSelector: "[data-admin-inventory-pagination]",
        endpoint: "/api/admin/inventory",
        renderMetrics: renderInventoryMetrics,
        renderRows: renderInventory
    });
}

function initAdminOrdersPage() {
    initPagedAdminListPage({
        pageSelector: "[data-admin-orders-page]",
        bodySelector: "[data-admin-orders-body]",
        messageSelector: "[data-admin-orders-message]",
        metricsSelector: "[data-admin-orders-metrics]",
        refreshSelector: "[data-admin-orders-refresh]",
        pageSizeSelector: "[data-admin-orders-page-size]",
        paginationSelector: "[data-admin-orders-pagination]",
        endpoint: "/api/admin/orders",
        renderMetrics: renderOrderMetrics,
        renderRows: renderOrders
    });
}

function initPagedAdminListPage(config) {
    const page = document.querySelector(config.pageSelector);
    if (!page) {
        return;
    }

    const state = {
        page: 1,
        pageSize: 10
    };

    const body = page.querySelector(config.bodySelector);
    const message = page.querySelector(config.messageSelector);
    const metrics = document.querySelector(config.metricsSelector);
    const refreshButton = document.querySelector(config.refreshSelector);
    const pageSizeSelect = page.querySelector(config.pageSizeSelector);
    const pagination = page.querySelector(config.paginationSelector);

    refreshButton?.addEventListener("click", () => loadPage());
    pageSizeSelect?.addEventListener("change", () => {
        state.page = 1;
        state.pageSize = Number(pageSizeSelect.value) || 10;
        void loadPage();
    });

    void loadPage();

    async function loadPage(pageNumber = state.page) {
        state.page = pageNumber;
        showMessage(message, "載入中...", "neutral");

        try {
            const data = await adminApi.json(`${config.endpoint}?page=${state.page}&pageSize=${state.pageSize}`);
            state.page = Number(adminApi.read(data, "page", "Page")) || state.page;
            state.pageSize = Number(adminApi.read(data, "pageSize", "PageSize")) || state.pageSize;
            config.renderMetrics(metrics, data);
            config.renderRows(body, data);
            renderPagination(pagination, data, loadPage);
            hideMessage(message);
        } catch (error) {
            showMessage(message, error.message, "danger");
        }
    }
}

function renderProductMetrics(container, data) {
    if (!container) {
        return;
    }

    const products = getArray(data, "products", "Products");
    const activeProducts = products.filter((product) =>
        getText(product, "status", "Status").toLowerCase() === "active").length;
    const skuCount = products.reduce((sum, product) =>
        sum + getNumber(product, "skuCount", "SkuCount"), 0);
    const categories = getArray(data, "categoryOptions", "CategoryOptions").length;

    container.replaceChildren(
        statCard("商品總數", String(getNumber(data, "totalCount", "TotalCount")), "Products", "neutral"),
        statCard("本頁啟用", String(activeProducts), "Active", "success"),
        statCard("本頁 SKU", String(skuCount), "SKUs", "warning"),
        statCard("分類數", String(categories), "Categories", "neutral")
    );
}

function renderSkuMetrics(container, data) {
    if (!container) {
        return;
    }

    const skus = getArray(data, "skus", "Skus");
    const activeSkus = skus.filter((sku) =>
        getText(sku, "status", "Status").toLowerCase() === "active").length;
    const availableQty = skus.reduce((sum, sku) =>
        sum + getNumber(sku, "totalAvailableQty", "TotalAvailableQty"), 0);
    const reservedQty = skus.reduce((sum, sku) =>
        sum + getNumber(sku, "totalReservedQty", "TotalReservedQty"), 0);

    container.replaceChildren(
        statCard("SKU 總數", String(getNumber(data, "totalCount", "TotalCount")), "ProductSkus", "neutral"),
        statCard("本頁啟用", String(activeSkus), "Active", "success"),
        statCard("本頁可售", formatNumber(availableQty), "Available", availableQty <= 0 ? "danger" : "success"),
        statCard("本頁保留", formatNumber(reservedQty), "Reserved", reservedQty > 0 ? "warning" : "neutral")
    );
}

function renderInventoryMetrics(container, data) {
    if (!container) {
        return;
    }

    const stocks = getArray(data, "stocks", "Stocks");
    const lowStock = stocks.filter((stock) =>
        getText(stock, "stockStatus", "StockStatus") === "LowStock").length;
    const outOfStock = stocks.filter((stock) =>
        getText(stock, "stockStatus", "StockStatus") === "OutOfStock").length;
    const warehouses = getArray(data, "warehouseOptions", "WarehouseOptions").length;

    container.replaceChildren(
        statCard("庫存列數", String(getNumber(data, "totalCount", "TotalCount")), "InventoryStocks", "neutral"),
        statCard("倉庫數", String(warehouses), "Warehouses", "neutral"),
        statCard("低庫存", String(lowStock), "LowStock", lowStock > 0 ? "warning" : "success"),
        statCard("缺貨", String(outOfStock), "OutOfStock", outOfStock > 0 ? "danger" : "success")
    );
}

function renderOrderMetrics(container, data) {
    if (!container) {
        return;
    }

    const orders = getArray(data, "orders", "Orders");
    const pendingOrders = orders.filter((order) =>
        getText(order, "orderStatus", "OrderStatus").toLowerCase() === "pending").length;
    const paidOrders = orders.filter((order) =>
        getText(order, "paymentStatus", "PaymentStatus").toLowerCase() === "paid").length;
    const pageAmount = orders.reduce((sum, order) =>
        sum + getNumber(order, "totalAmount", "TotalAmount"), 0);

    container.replaceChildren(
        statCard("訂單總數", String(getNumber(data, "totalCount", "TotalCount")), "Orders", "neutral"),
        statCard("本頁待處理", String(pendingOrders), "Pending", pendingOrders > 0 ? "warning" : "success"),
        statCard("本頁已付款", String(paidOrders), "Paid", "success"),
        statCard("本頁金額", formatCurrency(pageAmount), "TotalAmount", "neutral")
    );
}

function renderProducts(container, data) {
    const products = getArray(data, "products", "Products");
    if (!container) {
        return;
    }

    if (products.length === 0) {
        container.replaceChildren(emptyRow(8, "目前沒有商品資料。"));
        return;
    }

    container.replaceChildren(...products.map((product) => {
        const row = document.createElement("tr");
        const productId = adminApi.read(product, "productId", "ProductId");
        const productName = stackedText(
            getText(product, "productName", "ProductName"),
            getText(product, "brandName", "BrandName"));
        const skuText = `${getNumber(product, "activeSkuCount", "ActiveSkuCount")} / ${getNumber(product, "skuCount", "SkuCount")}`;
        const status = getText(product, "status", "Status");
        const editLink = document.createElement("a");
        editLink.className = "admin-table-action";
        editLink.href = `/admin/products/edit?id=${productId}`;
        editLink.textContent = "編輯";

        row.append(
            tableCell(getText(product, "productNo", "ProductNo")),
            tableCell(productName),
            tableCell(getText(product, "categoryName", "CategoryName")),
            tableCell(skuText),
            tableCell(formatPriceRange(product)),
            tableCell(badge(status || "Unknown", commerceStatusVariant(status))),
            tableCell(formatOptionalDate(getText(product, "updatedAt", "UpdatedAt") || getText(product, "createdAt", "CreatedAt"))),
            tableCell(editLink)
        );

        return row;
    }));
}

function renderSkus(container, data) {
    const skus = getArray(data, "skus", "Skus");
    if (!container) {
        return;
    }

    if (skus.length === 0) {
        container.replaceChildren(emptyRow(8, "目前沒有 SKU 資料。"));
        return;
    }

    container.replaceChildren(...skus.map((sku) => {
        const row = document.createElement("tr");
        const product = stackedText(
            getText(sku, "productName", "ProductName"),
            getText(sku, "productNo", "ProductNo"));
        const inventory = stackedText(
            `可售 ${formatNumber(getNumber(sku, "totalAvailableQty", "TotalAvailableQty"))}`,
            `實際 ${formatNumber(getNumber(sku, "totalOnHandQty", "TotalOnHandQty"))} / 保留 ${formatNumber(getNumber(sku, "totalReservedQty", "TotalReservedQty"))}`);
        const status = getText(sku, "status", "Status");

        row.append(
            tableCell(getText(sku, "skuNo", "SkuNo")),
            tableCell(product),
            tableCell(getText(sku, "skuName", "SkuName")),
            tableCell(getText(sku, "specText", "SpecText") || "未設定"),
            tableCell(formatCurrency(getNumber(sku, "salePrice", "SalePrice"))),
            tableCell(inventory),
            tableCell(badge(status || "Unknown", commerceStatusVariant(status))),
            tableCell(formatOptionalDate(getText(sku, "updatedAt", "UpdatedAt") || getText(sku, "createdAt", "CreatedAt")))
        );

        return row;
    }));
}

function renderInventory(container, data) {
    const stocks = getArray(data, "stocks", "Stocks");
    if (!container) {
        return;
    }

    if (stocks.length === 0) {
        container.replaceChildren(emptyRow(9, "目前沒有庫存資料。"));
        return;
    }

    container.replaceChildren(...stocks.map((stock) => {
        const row = document.createElement("tr");
        const warehouse = stackedText(
            getText(stock, "warehouseName", "WarehouseName"),
            getText(stock, "warehouseCode", "WarehouseCode"));
        const sku = stackedText(
            getText(stock, "skuNo", "SkuNo"),
            getText(stock, "skuName", "SkuName"));
        const status = getText(stock, "stockStatus", "StockStatus");

        row.append(
            tableCell(warehouse),
            tableCell(sku),
            tableCell(getText(stock, "productName", "ProductName")),
            tableCell(formatNumber(getNumber(stock, "onHandQty", "OnHandQty"))),
            tableCell(formatNumber(getNumber(stock, "reservedQty", "ReservedQty"))),
            tableCell(formatNumber(getNumber(stock, "availableQty", "AvailableQty"))),
            tableCell(formatNumber(getNumber(stock, "safetyStockQty", "SafetyStockQty"))),
            tableCell(badge(stockStatusText(status), commerceStatusVariant(status))),
            tableCell(formatOptionalDate(getText(stock, "updatedAt", "UpdatedAt")))
        );

        return row;
    }));
}

function renderOrders(container, data) {
    const orders = getArray(data, "orders", "Orders");
    if (!container) {
        return;
    }

    if (orders.length === 0) {
        container.replaceChildren(emptyRow(9, "目前沒有訂單資料。"));
        return;
    }

    container.replaceChildren(...orders.map((order) => {
        const row = document.createElement("tr");
        const customer = stackedText(
            getText(order, "customerName", "CustomerName"),
            `User#${adminApi.read(order, "userId", "UserId") ?? ""}`);
        const orderStatus = getText(order, "orderStatus", "OrderStatus");
        const paymentStatus = getText(order, "paymentStatus", "PaymentStatus");
        const shippingStatus = getText(order, "shippingStatus", "ShippingStatus");

        row.append(
            tableCell(getText(order, "orderNo", "OrderNo")),
            tableCell(customer),
            tableCell(formatCurrency(getNumber(order, "totalAmount", "TotalAmount"))),
            tableCell(`${formatNumber(getNumber(order, "itemCount", "ItemCount"))} 件`),
            tableCell(badge(paymentStatus || "Unknown", commerceStatusVariant(paymentStatus))),
            tableCell(badge(shippingStatus || "Unknown", commerceStatusVariant(shippingStatus))),
            tableCell(badge(orderStatus || "Unknown", commerceStatusVariant(orderStatus))),
            tableCell(formatOptionalDate(getText(order, "orderedAt", "OrderedAt"))),
            tableCell(formatOptionalDate(getText(order, "paidAt", "PaidAt")))
        );

        return row;
    }));
}

function renderAuditMetrics(container, data) {
    if (!container) {
        return;
    }

    const adminActions = getArray(data, "adminActions", "AdminActions");
    const dataChanges = getArray(data, "dataChanges", "DataChanges");
    const systemErrors = getArray(data, "systemErrors", "SystemErrors");
    const criticalErrors = systemErrors.filter((item) => {
        const level = getText(item, "errorLevel", "ErrorLevel").toLowerCase();
        return level === "critical" || level === "error";
    }).length;

    container.replaceChildren(
        statCard("後台操作", String(adminActions.length), "AdminActionLogs", "neutral"),
        statCard("資料異動", String(dataChanges.length), "AuditLogs", "warning"),
        statCard("系統錯誤", String(systemErrors.length), "SystemErrorLogs", criticalErrors > 0 ? "danger" : "neutral"),
        statCard("高風險錯誤", String(criticalErrors), "Error / Critical", criticalErrors > 0 ? "danger" : "success")
    );
}

function renderAdminActionLogs(container, data) {
    const rows = getArray(data, "adminActions", "AdminActions");
    if (!container) {
        return;
    }

    if (rows.length === 0) {
        container.replaceChildren(emptyRow(7, "目前沒有後台操作紀錄。"));
        return;
    }

    container.replaceChildren(...rows.map((log) => {
        const target = buildTargetText(
            getText(log, "targetType", "TargetType"),
            getText(log, "targetId", "TargetId"));
        const row = document.createElement("tr");

        row.append(
            tableCell(formatAuditDate(getText(log, "createdAt", "CreatedAt"))),
            tableCell(getText(log, "adminDisplayName", "AdminDisplayName") || `User#${adminApi.read(log, "userId", "UserId")}`),
            tableCell(getText(log, "moduleName", "ModuleName")),
            tableCell(getText(log, "actionName", "ActionName")),
            tableCell(target),
            tableCell(maskIpAddress(getText(log, "ipAddress", "IpAddress"))),
            tableCell(truncateText(getText(log, "description", "Description"), 96))
        );

        return row;
    }));
}

function renderDataChangeLogs(container, data) {
    const rows = getArray(data, "dataChanges", "DataChanges");
    if (!container) {
        return;
    }

    if (rows.length === 0) {
        container.replaceChildren(emptyRow(5, "目前沒有資料異動紀錄。"));
        return;
    }

    container.replaceChildren(...rows.map((log) => {
        const row = document.createElement("tr");
        row.append(
            tableCell(formatAuditDate(getText(log, "changedAt", "ChangedAt"))),
            tableCell(getText(log, "tableName", "TableName")),
            tableCell(getText(log, "recordId", "RecordId")),
            tableCell(auditBadge(getText(log, "actionType", "ActionType"))),
            tableCell(getText(log, "changedByDisplayName", "ChangedByDisplayName") || userIdText(adminApi.read(log, "changedBy", "ChangedBy")))
        );

        return row;
    }));
}

function renderSystemErrorLogs(container, data) {
    const rows = getArray(data, "systemErrors", "SystemErrors");
    if (!container) {
        return;
    }

    if (rows.length === 0) {
        container.replaceChildren(emptyRow(5, "目前沒有系統錯誤紀錄。"));
        return;
    }

    container.replaceChildren(...rows.map((log) => {
        const row = document.createElement("tr");
        const level = getText(log, "errorLevel", "ErrorLevel");
        row.append(
            tableCell(formatAuditDate(getText(log, "createdAt", "CreatedAt"))),
            tableCell(badge(level, errorLevelVariant(level))),
            tableCell(getText(log, "source", "Source") || "未記錄"),
            tableCell(getText(log, "requestPath", "RequestPath") || "未記錄"),
            tableCell(truncateText(getText(log, "message", "Message"), 96))
        );

        return row;
    }));
}

function renderSettingsMetrics(container, data) {
    if (!container) {
        return;
    }

    const sections = getArray(data, "sections", "Sections");
    const items = sections.flatMap((section) => getArray(section, "items", "Items"));
    const implementedCount = items.filter((item) => Boolean(adminApi.read(item, "isImplemented", "IsImplemented"))).length;
    const pendingCount = Math.max(items.length - implementedCount, 0);
    const mediumOrHigher = items.filter((item) => {
        const risk = getText(item, "riskLevel", "RiskLevel").toLowerCase();
        return risk === "medium" || risk === "high" || risk === "critical";
    }).length;

    container.replaceChildren(
        statCard("設定區塊", String(sections.length), "Sections", "neutral"),
        statCard("已實作", String(implementedCount), "Implemented", "success"),
        statCard("待建置", String(pendingCount), "NotConfigured", pendingCount > 0 ? "warning" : "success"),
        statCard("中高風險", String(mediumOrHigher), "Medium+", mediumOrHigher > 0 ? "warning" : "success")
    );
}

function renderSettingsSections(container, data) {
    if (!container) {
        return;
    }

    const sections = getArray(data, "sections", "Sections");
    if (sections.length === 0) {
        container.replaceChildren(emptyBlock("目前沒有系統設定資料。"));
        return;
    }

    container.replaceChildren(...sections.map(settingsSectionCard));
}

function settingsSectionCard(section) {
    const article = document.createElement("article");
    article.className = "admin-settings-section";

    const header = document.createElement("header");
    header.className = "admin-settings-section-header";

    const titleGroup = document.createElement("div");
    const title = document.createElement("h2");
    title.textContent = getText(section, "sectionName", "SectionName");
    const code = document.createElement("span");
    code.className = "admin-code";
    code.textContent = getText(section, "sectionKey", "SectionKey");
    titleGroup.append(title, code);

    const items = getArray(section, "items", "Items");
    header.append(titleGroup, badge(`${items.length} 項`, "neutral"));

    const list = document.createElement("div");
    list.className = "admin-settings-list";
    list.append(...items.map(settingsItemRow));

    article.append(header, list);
    return article;
}

function settingsItemRow(item) {
    const row = document.createElement("section");
    row.className = "admin-settings-item";

    const title = document.createElement("div");
    title.className = "admin-settings-item-title";

    const name = document.createElement("strong");
    name.textContent = getText(item, "name", "Name");
    const key = document.createElement("span");
    key.className = "admin-code";
    key.textContent = getText(item, "key", "Key");
    title.append(name, key);

    const value = document.createElement("p");
    value.className = "admin-settings-value";
    value.textContent = getText(item, "value", "Value") || "未設定";

    const description = document.createElement("p");
    description.className = "admin-settings-description";
    description.textContent = getText(item, "description", "Description") || "未提供說明";

    const meta = document.createElement("div");
    meta.className = "admin-role-meta";
    const risk = getText(item, "riskLevel", "RiskLevel");
    const implemented = Boolean(adminApi.read(item, "isImplemented", "IsImplemented"));
    meta.append(
        badge(implemented ? "已實作" : "待建置", implemented ? "success" : "warning"),
        badge(risk || "unknown", riskVariant(risk))
    );

    row.append(title, value, description, meta);
    return row;
}

function renderUsers(container, messageNode, data, reload) {
    const users = getArray(data, "users", "Users");
    const roleOptions = getArray(data, "roleOptions", "RoleOptions");
    const statuses = getArray(data, "availableStatuses", "AvailableStatuses");

    if (users.length === 0) {
        container.replaceChildren(emptyRow(7, "目前沒有可管理的使用者。"));
        return;
    }

    container.replaceChildren(...users.map((user) => userRow(user, roleOptions, statuses, messageNode, reload)));
}

function renderPagination(container, data, loadPage) {
    if (!container) {
        return;
    }

    const page = Number(adminApi.read(data, "page", "Page")) || 1;
    const totalPages = Number(adminApi.read(data, "totalPages", "TotalPages")) || 1;
    const totalCount = Number(adminApi.read(data, "totalCount", "TotalCount")) || 0;
    const pageSize = Number(adminApi.read(data, "pageSize", "PageSize")) || 10;
    const summary = document.createElement("span");
    summary.className = "admin-pagination-summary";
    summary.textContent = `第 ${page} / ${totalPages} 頁，共 ${totalCount} 筆，每頁 ${pageSize} 筆`;

    const prev = button("上一頁", "admin-table-action");
    prev.disabled = page <= 1;
    prev.addEventListener("click", () => loadPage(page - 1));

    const next = button("下一頁", "admin-table-action");
    next.disabled = page >= totalPages;
    next.addEventListener("click", () => loadPage(page + 1));

    container.replaceChildren(summary, prev, next);
}

function renderRoles(container, messageNode, data, reload) {
    const roles = getArray(data, "roles", "Roles");
    const permissionGroups = getArray(data, "permissionGroups", "PermissionGroups");

    if (roles.length === 0) {
        container.replaceChildren(emptyBlock("目前沒有角色資料。"));
        return;
    }

    container.replaceChildren(...roles.map((role) => roleCard(role, permissionGroups, messageNode, reload)));
}

function userRow(user, roleOptions, statuses, messageNode, reload) {
    const userId = adminApi.read(user, "userId", "UserId");
    const account = getText(user, "account", "Account");
    const displayName = getText(user, "displayName", "DisplayName");
    const userType = getText(user, "userType", "UserType");
    const status = getText(user, "status", "Status");
    const roleCodes = new Set(getArray(user, "roleCodes", "RoleCodes"));
    const row = document.createElement("tr");

    row.append(
        tableCell(account),
        tableCell(displayName),
        tableCell(userType),
        tableCell(statusSelect(statuses, status)),
        tableCell(roleSelect(roleOptions, roleCodes, userType)),
        tableCell(formatDate(getText(user, "lastLoginAt", "LastLoginAt"))),
        tableCell(userActions(row, userId, userType, messageNode, reload))
    );

    return row;
}

function statusSelect(statuses, selectedStatus) {
    const select = document.createElement("select");
    select.className = "admin-control";
    select.dataset.role = "status";

    for (const status of statuses) {
        const option = document.createElement("option");
        option.value = status;
        option.textContent = status;
        option.selected = status === selectedStatus;
        select.append(option);
    }

    return select;
}

function roleSelect(roleOptions, selectedCodes, userType) {
    const select = document.createElement("select");
    select.className = "admin-control";
    select.dataset.role = "roles";

    const empty = document.createElement("option");
    empty.value = "";
    empty.textContent = userType === "Customer" ? "前台會員不可指派後台角色" : "請選擇角色";
    empty.selected = selectedCodes.size === 0;
    select.append(empty);

    for (const role of roleOptions) {
        const code = getText(role, "roleCode", "RoleCode");
        const option = document.createElement("option");
        option.value = code;
        option.textContent = `${getText(role, "roleName", "RoleName")} (${code})`;
        option.selected = selectedCodes.has(code);
        select.append(option);
    }

    return select;
}

function userActions(row, userId, userType, messageNode, reload) {
    const wrap = document.createElement("div");
    wrap.className = "admin-row-actions";

    const saveStatus = button("儲存狀態", "admin-table-action");
    saveStatus.addEventListener("click", async () => {
        const status = row.querySelector("[data-role='status']")?.value;
        await runMutation(
            messageNode,
            () => adminApi.json(`/api/admin/system/users/${userId}/status`, {
                method: "PATCH",
                body: { status, reason: "後台使用者管理頁面調整" }
            }),
            reload);
    });

    const saveRoles = button("儲存角色", "admin-table-action");
    saveRoles.addEventListener("click", async () => {
        const selectedRole = row.querySelector("[data-role='roles']")?.value ?? "";

        if (userType === "Customer" && selectedRole) {
            await notify("前台會員不可指派後台角色", "請先建立正式的後台人員帳號，避免將 Customer 直接提升為 Admin 或 Staff。", "warning");
            return;
        }

        const roleCodes = selectedRole ? [selectedRole] : [];

        await runMutation(
            messageNode,
            () => adminApi.json(`/api/admin/system/users/${userId}/roles`, {
                method: "PUT",
                body: { roleCodes, reason: "後台使用者管理頁面調整" }
            }),
            reload);
    });

    wrap.append(saveStatus, saveRoles);
    return wrap;
}

function roleCard(role, permissionGroups, messageNode, reload) {
    const roleId = adminApi.read(role, "roleId", "RoleId");
    const roleCode = getText(role, "roleCode", "RoleCode");
    const isSuperAdmin = roleCode.toUpperCase() === "SUPER_ADMIN";
    const permissions = new Set(getArray(role, "permissions", "Permissions"));
    const details = document.createElement("details");
    details.className = "admin-role-card";

    const summary = document.createElement("summary");
    summary.className = "admin-role-summary";

    const title = document.createElement("div");
    const h2 = document.createElement("h2");
    h2.textContent = getText(role, "roleName", "RoleName");
    const code = document.createElement("span");
    code.className = "admin-code";
    code.textContent = roleCode;
    title.append(h2, code);

    const meta = document.createElement("div");
    meta.className = "admin-role-meta";
    meta.append(
        badge(`${adminApi.read(role, "userCount", "UserCount") ?? 0} 人`, "neutral"),
        badge(adminApi.read(role, "isSystemRole", "IsSystemRole") ? "系統角色" : "自訂角色", "warning"),
        badge("展開調整", "blue")
    );
    summary.append(title, meta);

    const content = document.createElement("div");
    content.className = "admin-role-card-content";

    const description = document.createElement("p");
    description.className = "admin-role-description";
    description.textContent = getText(role, "description", "Description") || "未提供說明";

    const matrix = document.createElement("div");
    matrix.className = "admin-permission-matrix";
    matrix.dataset.role = "permissions";

    for (const group of permissionGroups) {
        matrix.append(permissionGroup(group, permissions, isSuperAdmin));
    }

    const footer = document.createElement("footer");
    footer.className = "admin-role-card-footer";
    const save = button("儲存權限", "admin-primary-button");
    save.disabled = isSuperAdmin;
    save.addEventListener("click", async () => {
        const permissionCodes = Array.from(matrix.querySelectorAll("input[type='checkbox']:checked"))
            .map((input) => input.value);

        await runMutation(
            messageNode,
            () => adminApi.json(`/api/admin/system/roles/${roleId}/permissions`, {
                method: "PUT",
                body: { permissionCodes, reason: "後台角色權限管理頁面調整" }
            }),
            reload);
    });
    footer.append(save);

    content.append(description, matrix, footer);
    details.append(summary, content);
    return details;
}

function permissionGroup(group, selectedPermissions, disabled) {
    const section = document.createElement("section");
    section.className = "admin-permission-group";

    const title = document.createElement("h3");
    title.textContent = getText(group, "moduleName", "ModuleName");
    section.append(title);

    for (const permission of getArray(group, "permissions", "Permissions")) {
        const code = getText(permission, "permissionCode", "PermissionCode");
        const label = document.createElement("label");
        label.className = "admin-check-row";

        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.value = code;
        checkbox.checked = selectedPermissions.has(code);
        checkbox.disabled = disabled;

        const text = document.createElement("span");
        text.textContent = `${getText(permission, "permissionName", "PermissionName")} (${code})`;

        label.append(checkbox, text);
        section.append(label);
    }

    return section;
}

function buildTargetText(targetType, targetId) {
    if (!targetType && !targetId) {
        return "未記錄";
    }

    if (!targetId) {
        return targetType;
    }

    if (!targetType) {
        return targetId;
    }

    return `${targetType} #${targetId}`;
}

function auditBadge(actionType) {
    const normalized = (actionType || "").toLowerCase();
    if (normalized === "insert") {
        return badge(actionType, "success");
    }

    if (normalized === "delete") {
        return badge(actionType, "danger");
    }

    if (normalized === "update") {
        return badge(actionType, "warning");
    }

    return badge(actionType || "Unknown", "neutral");
}

function errorLevelVariant(level) {
    const normalized = (level || "").toLowerCase();
    if (normalized === "critical" || normalized === "error") {
        return "danger";
    }

    if (normalized === "warning") {
        return "warning";
    }

    if (normalized === "info") {
        return "success";
    }

    return "neutral";
}

function riskVariant(riskLevel) {
    const normalized = (riskLevel || "").toLowerCase();
    if (normalized === "critical" || normalized === "high") {
        return "danger";
    }

    if (normalized === "medium") {
        return "warning";
    }

    if (normalized === "low") {
        return "success";
    }

    return "neutral";
}

function userIdText(userId) {
    return userId ? `User#${userId}` : "系統";
}

function maskIpAddress(value) {
    if (!value) {
        return "未記錄";
    }

    const parts = value.split(".");
    if (parts.length === 4) {
        return `${parts[0]}.${parts[1]}.*.*`;
    }

    if (value.length <= 8) {
        return value;
    }

    return `${value.slice(0, 8)}...`;
}

function truncateText(value, maxLength) {
    if (!value) {
        return "未記錄";
    }

    if (value.length <= maxLength) {
        return value;
    }

    return `${value.slice(0, Math.max(maxLength - 3, 0))}...`;
}

function formatAuditDate(value) {
    if (!value) {
        return "未記錄";
    }

    return formatDate(value);
}

async function runMutation(messageNode, action, reload) {
    showMessage(messageNode, "儲存中。", "neutral");

    try {
        const result = await action();
        showMessage(messageNode, getText(result, "message", "Message") || "儲存完成。", "success");
        await reload();
    } catch (error) {
        hideMessage(messageNode);
        await notify("操作未完成", error.message, "warning");
    }
}

async function notify(title, text, icon) {
    if (window.Swal) {
        await window.Swal.fire({
            title,
            text,
            icon,
            confirmButtonText: "知道了"
        });
        return;
    }

    alert(`${title}\n${text}`);
}

function statCard(label, value, note, variant) {
    const card = document.createElement("article");
    card.className = `admin-stat-card admin-stat-${variant}`;

    const labelEl = document.createElement("div");
    labelEl.className = "admin-stat-label";
    labelEl.textContent = label;

    const valueEl = document.createElement("div");
    valueEl.className = "admin-stat-value";
    valueEl.textContent = value;

    const noteEl = document.createElement("div");
    noteEl.className = "admin-stat-note";
    noteEl.textContent = note;

    card.append(labelEl, valueEl, noteEl);
    return card;
}

function tableCell(content) {
    const cell = document.createElement("td");
    if (content instanceof Node) {
        cell.append(content);
    } else {
        cell.textContent = content ?? "";
    }

    return cell;
}

function button(text, className) {
    const element = document.createElement("button");
    element.type = "button";
    element.className = className;
    element.textContent = text;
    return element;
}

function badge(text, variant) {
    const element = document.createElement("span");
    element.className = `admin-badge admin-badge-${variant}`;
    element.textContent = text;
    return element;
}

function emptyRow(colspan, text) {
    const row = document.createElement("tr");
    const cell = document.createElement("td");
    cell.colSpan = colspan;
    cell.append(emptyBlock(text));
    row.append(cell);
    return row;
}

function emptyBlock(text) {
    const block = document.createElement("div");
    block.className = "admin-empty-state";
    const strong = document.createElement("strong");
    strong.textContent = text;
    block.append(strong);
    return block;
}

function showMessage(node, text, variant) {
    if (!node) {
        return;
    }

    node.hidden = false;
    node.className = `admin-inline-alert admin-inline-alert-${variant}`;
    node.textContent = text;
}

function hideMessage(node) {
    if (node) {
        node.hidden = true;
    }
}

function getArray(payload, camelName, pascalName) {
    return adminApi.read(payload, camelName, pascalName) ?? [];
}

function getText(payload, camelName, pascalName) {
    return adminApi.read(payload, camelName, pascalName) ?? "";
}

function getNumber(payload, camelName, pascalName) {
    const value = adminApi.read(payload, camelName, pascalName);
    const number = Number(value);
    return Number.isFinite(number) ? number : 0;
}

function readNumber(value, fallback) {
    const number = Number(value);
    return Number.isFinite(number) ? number : fallback;
}

function readNullableNumber(value) {
    if (value === null || value === undefined || value === "") {
        return null;
    }

    const number = Number(value);
    return Number.isFinite(number) ? number : null;
}

function numberOrEmpty(value) {
    if (value === null || value === undefined) {
        return "";
    }

    const number = Number(value);
    return Number.isFinite(number) ? String(number) : "";
}

function fillSimpleSelect(select, values, selectedValue) {
    if (!select) {
        return;
    }

    select.replaceChildren(...values.map((value) => {
        const option = document.createElement("option");
        option.value = value;
        option.textContent = value;
        option.selected = value === selectedValue;
        return option;
    }));
}

function fillSelect(select, values, selectedValue, valueFactory, labelFactory) {
    if (!select) {
        return;
    }

    const options = values.map((value) => {
        const optionValue = valueFactory(value);
        const option = document.createElement("option");
        option.value = optionValue;
        option.textContent = labelFactory(value);
        option.selected = String(optionValue) === String(selectedValue ?? "");
        return option;
    });

    select.replaceChildren(...options);
}

function formatNumber(value) {
    const number = Number(value);
    if (!Number.isFinite(number)) {
        return "0";
    }

    return number.toLocaleString("zh-TW");
}

function formatCurrency(value) {
    const number = Number(value);
    if (!Number.isFinite(number)) {
        return "未設定";
    }

    return number.toLocaleString("zh-TW", {
        style: "currency",
        currency: "TWD",
        maximumFractionDigits: 0
    });
}

function formatOptionalDate(value) {
    return value ? formatDate(value) : "未記錄";
}

function formatPriceRange(product) {
    const minValue = adminApi.read(product, "minSalePrice", "MinSalePrice");
    const maxValue = adminApi.read(product, "maxSalePrice", "MaxSalePrice");

    if (minValue === null || minValue === undefined || maxValue === null || maxValue === undefined) {
        return "未設定";
    }

    const min = Number(minValue);
    const max = Number(maxValue);

    if (!Number.isFinite(min) || !Number.isFinite(max)) {
        return "未設定";
    }

    if (min === max) {
        return formatCurrency(min);
    }

    return `${formatCurrency(min)} - ${formatCurrency(max)}`;
}

function stackedText(primary, secondary) {
    const wrapper = document.createElement("div");

    const main = document.createElement("strong");
    main.textContent = primary || "未命名";
    wrapper.append(main);

    if (secondary) {
        const sub = document.createElement("div");
        sub.className = "admin-code";
        sub.textContent = secondary;
        wrapper.append(sub);
    }

    return wrapper;
}

function commerceStatusVariant(status) {
    const normalized = (status || "").toLowerCase();
    if (["active", "paid", "completed", "shipped", "delivered", "instock"].includes(normalized)) {
        return "success";
    }

    if (["pending", "processing", "preparing", "draft", "lowstock", "partialrefunded"].includes(normalized)) {
        return "warning";
    }

    if (["inactive", "cancelled", "failed", "refunded", "returned", "outofstock"].includes(normalized)) {
        return "danger";
    }

    return "neutral";
}

function stockStatusText(status) {
    const normalized = (status || "").toLowerCase();
    if (normalized === "instock") {
        return "庫存正常";
    }

    if (normalized === "lowstock") {
        return "低庫存";
    }

    if (normalized === "outofstock") {
        return "缺貨";
    }

    return status || "Unknown";
}

function formatDate(value) {
    if (!value) {
        return "尚未登入";
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return date.toLocaleString("zh-TW", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    });
}
