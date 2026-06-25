// FreshMart frontend interactions for MVC Razor Views.

function pick(source, camelName, pascalName) {
  return source?.[camelName] ?? source?.[pascalName];
}

function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function isServerRenderedGrid(grid) {
  return grid?.dataset.freshmartServerRendered === "true";
}

function toggleFav(btn) {
  btn.textContent = btn.textContent === "🤍" ? "❤️" : "🤍";
}

function filterServerRenderedProducts(grid, cat) {
  let visibleCount = 0;

  grid.querySelectorAll("[data-freshmart-product-category]").forEach((card) => {
    const category = card.dataset.freshmartProductCategory || "";
    const isHidden = cat !== "全部" && category !== cat && !category.includes(cat);
    card.hidden = isHidden;

    if (!isHidden) {
      visibleCount += 1;
    }
  });

  updateProductCatalogState(visibleCount);
}

function filterCategory(el, cat) {
  document.querySelectorAll(".pill").forEach((pill) => pill.classList.remove("active"));
  el?.classList.add("active");

  const grid = document.getElementById("productsGrid");
  if (isServerRenderedGrid(grid)) {
    filterServerRenderedProducts(grid, cat);
  }
}

function updateProductCatalogState(visibleCount) {
  const countElement = document.getElementById("freshmartProductCount");
  const emptyState = document.querySelector("[data-freshmart-empty-state]");

  if (countElement) {
    countElement.innerHTML = `顯示 <strong>${visibleCount}</strong> 項商品`;
  }

  if (emptyState) {
    emptyState.hidden = visibleCount > 0;
  }
}

function getCartToken() {
  return document.querySelector("#freshmartDrawerTokenForm input[name='__RequestVerificationToken']")?.value
    || document.querySelector("#freshmartCartTokenForm input[name='__RequestVerificationToken']")?.value
    || "";
}

function getProductCard(source) {
  return source?.closest?.("[data-product-id]");
}

function getProductFromCard(card) {
  if (!card) {
    return null;
  }

  return {
    id: Number.parseInt(card.dataset.productId || "0", 10),
    name: card.dataset.productName || "商品",
    emoji: card.dataset.productEmoji || "🛒"
  };
}

function getCartEndpoint() {
  const grid = document.getElementById("productsGrid");
  return grid?.dataset.cartAddUrl || "/cart/items";
}

function getReturnUrl() {
  const grid = document.getElementById("productsGrid");
  return grid?.dataset.cartReturnUrl || window.location.pathname + window.location.search;
}

async function fetchJson(url, options) {
  const response = await fetch(url, {
    credentials: "same-origin",
    ...options,
    headers: {
      "X-Requested-With": "XMLHttpRequest",
      ...(options?.headers || {})
    }
  });

  if (response.redirected && response.url.includes("/account/login")) {
    window.location.assign(response.url);
    return null;
  }

  if (response.status === 401 || response.status === 403) {
    window.location.assign(`/account/login?returnUrl=${encodeURIComponent(getReturnUrl())}`);
    return null;
  }

  if (!response.ok) {
    throw new Error("FreshMart AJAX request failed.");
  }

  return response.json();
}

async function loadCartDrawer() {
  try {
    const cart = await fetchJson("/cart/drawer", { method: "GET" });
    if (cart) {
      renderCartDrawer(cart);
    }
  } catch {
    showToast("購物車資料載入失敗，請稍後再試");
  }
}

async function postCartItem(product) {
  const formData = new FormData();
  formData.append("__RequestVerificationToken", getCartToken());
  formData.append("ProductId", String(product.id));
  formData.append("Quantity", "1");
  formData.append("ReturnUrl", getReturnUrl());

  return fetchJson(getCartEndpoint(), {
    method: "POST",
    body: formData
  });
}

async function addToCartFromButton(button) {
  const card = getProductCard(button);
  const product = getProductFromCard(card);
  if (!product?.id) {
    showToast("商品資料尚未同步，請稍後再試");
    return;
  }

  button.disabled = true;
  try {
    const result = await postCartItem(product);
    if (!result) {
      return;
    }

    if (!pick(result, "succeeded", "Succeeded")) {
      showToast(pick(result, "message", "Message") || "加入購物車失敗，請稍後再試");
      return;
    }

    const cart = pick(result, "cart", "Cart");
    if (cart) {
      renderCartDrawer(cart);
    } else {
      await loadCartDrawer();
    }
    showToast(pick(result, "message", "Message") || `${product.emoji} ${product.name} 已加入購物車`);
  } catch {
    showToast("加入購物車失敗，請稍後再試");
  } finally {
    button.disabled = false;
  }
}

async function updateCartQuantity(cartItemId, quantity) {
  if (quantity <= 0) {
    await removeCartItem(cartItemId);
    return;
  }

  const formData = new FormData();
  formData.append("__RequestVerificationToken", getCartToken());
  formData.append("quantity", String(quantity));
  formData.append("returnUrl", getReturnUrl());

  await submitCartMutation(`/cart/items/${cartItemId}/quantity`, formData);
}

async function removeCartItem(cartItemId) {
  const formData = new FormData();
  formData.append("__RequestVerificationToken", getCartToken());
  formData.append("returnUrl", getReturnUrl());

  await submitCartMutation(`/cart/items/${cartItemId}/remove`, formData);
}

async function submitCartMutation(url, formData) {
  try {
    const result = await fetchJson(url, {
      method: "POST",
      body: formData
    });
    if (!result) {
      return;
    }

    if (!pick(result, "succeeded", "Succeeded")) {
      showToast(pick(result, "message", "Message") || "購物車更新失敗，請稍後再試");
      return;
    }

    const cart = pick(result, "cart", "Cart");
    if (cart) {
      renderCartDrawer(cart);
    } else {
      await loadCartDrawer();
    }
  } catch {
    showToast("購物車更新失敗，請稍後再試");
  }
}

function renderCartDrawer(cart) {
  const items = pick(cart, "items", "Items") || [];
  const itemCount = pick(cart, "itemCount", "ItemCount") ?? items.reduce((sum, item) => sum + Number(pick(item, "quantity", "Quantity") || 0), 0);
  const estimatedTotalText = pick(cart, "estimatedTotalText", "EstimatedTotalText") || "NT$0";

  const badges = document.querySelectorAll(".freshmart-cart-badge");
  const totalElement = document.getElementById("cartTotal");
  const itemsElement = document.getElementById("cartItems");

  badges.forEach((badge) => {
    badge.textContent = itemCount;
  });
  if (totalElement) {
    totalElement.textContent = estimatedTotalText;
  }
  if (!itemsElement) {
    return;
  }

  if (!items.length) {
    itemsElement.innerHTML = '<p style="color:var(--mid);font-size:.9rem;text-align:center;padding:3rem 0">購物車是空的，快去挑選商品吧 🛒</p>';
    return;
  }

  itemsElement.innerHTML = items.map((item) => {
    const cartItemId = pick(item, "cartItemId", "CartItemId");
    const productName = pick(item, "productName", "ProductName");
    const skuName = pick(item, "skuName", "SkuName");
    const quantity = Number(pick(item, "quantity", "Quantity") || 0);
    const unitPriceText = pick(item, "unitPriceText", "UnitPriceText");
    const lineTotalText = pick(item, "lineTotalText", "LineTotalText");

    return `
      <div class="cart-item">
        <div class="cart-item-emoji">🛒</div>
        <div class="cart-item-info">
          <div class="cart-item-name">${escapeHtml(productName)}</div>
          <div class="cart-item-price">${escapeHtml(skuName)} · ${escapeHtml(unitPriceText)}</div>
          <div class="qty-control">
            <button class="qty-btn" type="button" onclick="updateCartQuantity(${cartItemId}, ${quantity - 1})">−</button>
            <span class="qty-num">${quantity}</span>
            <button class="qty-btn" type="button" onclick="updateCartQuantity(${cartItemId}, ${quantity + 1})">+</button>
            <button class="cart-drawer-remove" type="button" onclick="removeCartItem(${cartItemId})">移除</button>
          </div>
        </div>
        <div class="cart-item-total">${escapeHtml(lineTotalText)}</div>
      </div>
    `;
  }).join("");
}

async function openCart() {
  document.getElementById("cartDrawer")?.classList.add("open");
  document.getElementById("drawerOverlay")?.classList.add("open");
  document.body.style.overflow = "hidden";
  await loadCartDrawer();
}

function closeCart() {
  document.getElementById("cartDrawer")?.classList.remove("open");
  document.getElementById("drawerOverlay")?.classList.remove("open");
  document.body.style.overflow = "";
}

function setFreshmartMobileMenu(open) {
  const menu = document.getElementById("freshmartMobileMenu");
  const toggle = document.getElementById("freshmartMobileMenuToggle");

  if (!menu || !toggle) {
    return;
  }

  if (open) {
    menu.hidden = false;
    menu.classList.add("open");
    toggle.setAttribute("aria-expanded", "true");
    toggle.setAttribute("aria-label", "關閉導覽選單");
    return;
  }

  menu.classList.remove("open");
  menu.hidden = true;
  toggle.setAttribute("aria-expanded", "false");
  toggle.setAttribute("aria-label", "開啟導覽選單");
}

function toggleFreshmartMobileMenu() {
  const menu = document.getElementById("freshmartMobileMenu");
  setFreshmartMobileMenu(!menu?.classList.contains("open"));
}

function closeFreshmartMobileMenu() {
  setFreshmartMobileMenu(false);
}

function openFreshmartShopSidebar() {
  const sidebar = document.getElementById("freshmartShopSidebar");
  sidebar?.classList.add("open");
  document.getElementById("freshmartShopOverlay")?.classList.add("open");

  if (window.matchMedia("(max-width: 1180px)").matches) {
    document.body.style.overflow = "hidden";
  } else {
    sidebar?.scrollIntoView({ behavior: "smooth", block: "nearest" });
  }
}

function closeFreshmartShopSidebar() {
  document.getElementById("freshmartShopSidebar")?.classList.remove("open");
  document.getElementById("freshmartShopOverlay")?.classList.remove("open");
  document.body.style.overflow = "";
}

let freshmartShopRequestController = null;

function getFreshmartShopRegion() {
  return document.querySelector("[data-freshmart-shop-ajax-region]");
}

function isFreshmartShopUrl(url) {
  return url.origin === window.location.origin
    && (url.pathname.toLowerCase() === "/products" || url.pathname.toLowerCase().startsWith("/products/"));
}

function buildFreshmartShopFormUrl(form) {
  const url = new URL(form.action || window.location.href, window.location.origin);
  const formData = new FormData(form);
  url.search = "";

  for (const [key, value] of formData.entries()) {
    const normalizedValue = String(value ?? "").trim();
    if (normalizedValue.length > 0) {
      url.searchParams.append(key, normalizedValue);
    }
  }

  return url;
}

function initializeFreshmartShopCatalog() {
  const grid = document.getElementById("productsGrid");
  if (isServerRenderedGrid(grid)) {
    updateProductCatalogState(grid.querySelectorAll("[data-freshmart-product-category]:not([hidden])").length);
  }
}

async function loadFreshmartShopUrl(url, options = {}) {
  const region = getFreshmartShopRegion();
  if (!region) {
    window.location.assign(url.toString());
    return;
  }

  freshmartShopRequestController?.abort();
  freshmartShopRequestController = new AbortController();

  region.classList.add("is-loading");
  region.setAttribute("aria-busy", "true");

  try {
    const response = await fetch(url, {
      method: "GET",
      credentials: "same-origin",
      signal: freshmartShopRequestController.signal,
      headers: {
        "X-Requested-With": "XMLHttpRequest"
      }
    });

    if (response.redirected) {
      window.location.assign(response.url);
      return;
    }

    if (!response.ok) {
      throw new Error("FreshMart shop update failed.");
    }

    const html = await response.text();
    const nextDocument = new DOMParser().parseFromString(html, "text/html");
    const nextRegion = nextDocument.querySelector("[data-freshmart-shop-ajax-region]");

    if (!nextRegion) {
      window.location.assign(url.toString());
      return;
    }

    region.replaceWith(document.importNode(nextRegion, true));
    document.title = nextDocument.title || document.title;

    if (options.pushState !== false) {
      history.pushState({ freshmartShopUrl: url.toString() }, "", url);
    }

    closeFreshmartShopSidebar();
    initializeFreshmartShopCatalog();
  } catch (error) {
    if (error.name === "AbortError") {
      return;
    }

    const currentRegion = getFreshmartShopRegion();
    currentRegion?.classList.remove("is-loading");
    currentRegion?.removeAttribute("aria-busy");
    showToast("商品資訊更新失敗，將改用完整頁面載入");
    window.location.assign(url.toString());
  }
}

function showToast(msg) {
  const toast = document.getElementById("toast");
  const toastMessage = document.getElementById("toastMsg");
  if (!toast || !toastMessage) {
    return;
  }

  toastMessage.textContent = msg;
  toast.classList.add("show");
  clearTimeout(window._toastTimer);
  window._toastTimer = setTimeout(() => toast.classList.remove("show"), 2400);
}

window.addEventListener("scroll", () => {
  document.getElementById("navbar")?.classList.toggle("scrolled", scrollY > 40);
  document.querySelectorAll(".fade-up").forEach((el) => {
    if (el.getBoundingClientRect().top < innerHeight - 80) {
      el.classList.add("visible");
    }
  });
});

function scrollToProducts() {
  document.getElementById("products")?.scrollIntoView({ behavior: "smooth" });
}

document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".fade-up").forEach((el) => {
    if (el.getBoundingClientRect().top < innerHeight - 80) {
      el.classList.add("visible");
    }
  });

  initializeFreshmartShopCatalog();
});

document.addEventListener("keydown", (event) => {
  if (event.key === "Escape") {
    closeFreshmartMobileMenu();
    closeFreshmartShopSidebar();
  }
});

document.addEventListener("click", (event) => {
  const navbar = document.getElementById("navbar");
  const menu = document.getElementById("freshmartMobileMenu");

  if (menu?.classList.contains("open") && navbar && !navbar.contains(event.target)) {
    closeFreshmartMobileMenu();
  }
});

document.addEventListener("submit", (event) => {
  const form = event.target?.closest?.("form[data-freshmart-shop-form]");
  if (!form) {
    return;
  }

  const url = buildFreshmartShopFormUrl(form);
  if (!isFreshmartShopUrl(url)) {
    return;
  }

  event.preventDefault();
  loadFreshmartShopUrl(url);
});

document.addEventListener("click", (event) => {
  const link = event.target?.closest?.("a[data-freshmart-shop-link]");
  if (!link
    || event.defaultPrevented
    || event.button !== 0
    || event.metaKey
    || event.ctrlKey
    || event.shiftKey
    || event.altKey
    || link.target) {
    return;
  }

  const url = new URL(link.href, window.location.origin);
  if (!isFreshmartShopUrl(url)) {
    return;
  }

  event.preventDefault();
  loadFreshmartShopUrl(url);
});

window.addEventListener("popstate", () => {
  const region = getFreshmartShopRegion();
  if (!region) {
    return;
  }

  const url = new URL(window.location.href);
  if (isFreshmartShopUrl(url)) {
    loadFreshmartShopUrl(url, { pushState: false });
  }
});
