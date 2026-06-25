(function () {
    const defaultTargetSelector = "#cart-content";

    function getTargetSelector(form) {
        return form.dataset.cartAjaxTarget || defaultTargetSelector;
    }

    function getTarget(selector) {
        return document.querySelector(selector);
    }

    function closeOpenModal() {
        const openModal = document.querySelector(".modal.show");
        if (!openModal || !window.bootstrap) {
            return;
        }

        const modal = window.bootstrap.Modal.getInstance(openModal);
        modal?.hide();
    }

    function cleanupModalArtifacts() {
        document.querySelectorAll(".modal-backdrop").forEach((backdrop) => backdrop.remove());
        document.body.classList.remove("modal-open");
        document.body.style.removeProperty("overflow");
        document.body.style.removeProperty("padding-right");
    }

    function parseResponseDocument(html) {
        const parser = new DOMParser();
        return parser.parseFromString(html, "text/html");
    }

    function setFormDisabled(form, disabled) {
        form.querySelectorAll("button, input, select, textarea").forEach((control) => {
            control.disabled = disabled;
        });
    }

    async function replaceAjaxTarget(responseDocument, targetSelector, responseUrl) {
        const nextTarget = responseDocument.querySelector(targetSelector);
        const currentTarget = getTarget(targetSelector);

        if (!nextTarget || !currentTarget) {
            window.location.assign(responseUrl || window.location.href);
            return;
        }

        closeOpenModal();
        currentTarget.replaceWith(nextTarget);
        cleanupModalArtifacts();
    }

    async function submitCartForm(form) {
        const formData = new FormData(form);
        const targetSelector = getTargetSelector(form);
        setFormDisabled(form, true);

        try {
            const response = await fetch(form.action, {
                method: form.method || "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                credentials: "same-origin",
                redirect: "follow"
            });

            if (!response.ok) {
                throw new Error("Cart request failed.");
            }

            const responseDocument = parseResponseDocument(await response.text());
            await replaceAjaxTarget(responseDocument, targetSelector, response.url);
        } catch {
            setFormDisabled(form, false);
            form.submit();
        } finally {
            setFormDisabled(form, false);
        }
    }

    function getNumber(value, fallback) {
        const parsed = Number.parseInt(value, 10);
        return Number.isFinite(parsed) ? parsed : fallback;
    }

    function setAddFeedback(form, succeeded, message) {
        const feedback = form.querySelector("[data-cart-add-feedback]");
        if (!feedback) {
            return;
        }

        feedback.textContent = message || "";
        feedback.classList.toggle("is-success", Boolean(succeeded));
        feedback.classList.toggle("is-error", !succeeded);
    }

    function updateAddToCartState(productId, quantityAdded) {
        document
            .querySelectorAll(`[data-cart-product-id="${productId}"][data-cart-current-quantity]`)
            .forEach((modal) => {
                const currentQuantity = getNumber(modal.dataset.cartCurrentQuantity, 0);
                const nextQuantity = currentQuantity + quantityAdded;
                modal.dataset.cartCurrentQuantity = String(nextQuantity);

                const existingQuantity = modal.querySelector("[data-cart-existing-quantity]");
                if (existingQuantity) {
                    existingQuantity.textContent = String(nextQuantity);
                }

                const existingRow = modal.querySelector("[data-cart-existing-row]");
                if (existingRow) {
                    existingRow.hidden = nextQuantity <= 0;
                }

                const stockCount = getNumber(
                    modal.querySelector("[data-cart-stock-count]")?.textContent,
                    0);
                const remainingQuantity = Math.max(stockCount - nextQuantity, 0);
                const quantityInput = modal.querySelector("[data-cart-add-quantity]");
                if (quantityInput) {
                    quantityInput.max = String(Math.max(remainingQuantity, 1));
                    quantityInput.value = "1";
                }

                if (remainingQuantity > 0) {
                    return;
                }

                document
                    .querySelectorAll(`[data-bs-target="#${modal.id}"]`)
                    .forEach((button) => {
                        button.disabled = true;
                        button.textContent = "已達庫存上限";
                    });
            });
    }

    function showCartAddedModal(message) {
        const modalElement = document.querySelector("#cartAddedModal");
        if (!modalElement || !window.bootstrap) {
            return;
        }

        const messageElement = modalElement.querySelector("[data-cart-added-message]");
        if (messageElement) {
            messageElement.textContent = message || "商品已加入購物車。";
        }

        window.bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }

    function resetAddModal(modal) {
        const form = modal.querySelector("[data-cart-add-form]");
        if (!form) {
            return;
        }

        const quantityInput = form.querySelector("[data-cart-add-quantity]");
        if (quantityInput) {
            quantityInput.value = "1";
        }

        setAddFeedback(form, true, "");
    }

    async function submitAddToCartForm(form) {
        const formData = new FormData(form);
        const productId = form.dataset.cartProductId || formData.get("ProductId");
        const quantity = getNumber(formData.get("Quantity"), 1);
        setFormDisabled(form, true);
        setAddFeedback(form, true, "");

        try {
            const response = await fetch(form.action, {
                method: form.method || "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                credentials: "same-origin"
            });

            if (!response.ok) {
                throw new Error("Add cart request failed.");
            }

            const result = await response.json();
            if (!result.succeeded) {
                setAddFeedback(form, false, result.message || "加入購物車失敗，請稍後再試。");
                return;
            }

            updateAddToCartState(productId, quantity);
            const modalElement = form.closest(".modal");
            if (modalElement && window.bootstrap) {
                const addModal = window.bootstrap.Modal.getOrCreateInstance(modalElement);
                modalElement.addEventListener(
                    "hidden.bs.modal",
                    () => showCartAddedModal(result.message),
                    { once: true });
                addModal.hide();
            } else {
                showCartAddedModal(result.message);
            }
        } catch {
            setAddFeedback(form, false, "加入購物車失敗，請稍後再試。");
        } finally {
            setFormDisabled(form, false);
        }
    }

    function updateCartSummary(result) {
        const subtotal = document.querySelector("[data-cart-summary='subtotal']");
        const discount = document.querySelector("[data-cart-summary='discount']");
        const estimated = document.querySelector("[data-cart-summary='estimated']");

        if (subtotal && result.subtotalText) {
            subtotal.textContent = result.subtotalText;
        }

        if (discount && result.discountTotalText) {
            discount.textContent = result.discountTotalText;
        }

        if (estimated && result.estimatedTotalText) {
            estimated.textContent = result.estimatedTotalText;
        }
    }

    function setCouponFeedback(form, result) {
        const feedback = form.querySelector("[data-cart-coupon-message]");
        if (!feedback) {
            return;
        }

        feedback.textContent = result.message || "優惠券試算未完成，請稍後再試。";
        feedback.classList.toggle("is-success", Boolean(result.succeeded));
        feedback.classList.toggle("is-error", !result.succeeded);
    }

    async function submitCouponForm(form) {
        const formData = new FormData(form);
        setFormDisabled(form, true);

        try {
            const response = await fetch(form.action, {
                method: form.method || "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                credentials: "same-origin"
            });

            if (!response.ok) {
                throw new Error("Coupon preview failed.");
            }

            const result = await response.json();
            updateCartSummary(result);
            setCouponFeedback(form, result);
        } catch {
            setCouponFeedback(form, {
                succeeded: false,
                message: "優惠券試算失敗，請稍後再試。"
            });
        } finally {
            setFormDisabled(form, false);
        }
    }

    document.addEventListener("submit", (event) => {
        const addToCartForm = event.target.closest("[data-cart-add-form]");
        if (addToCartForm) {
            event.preventDefault();
            submitAddToCartForm(addToCartForm);
            return;
        }

        const couponForm = event.target.closest("[data-cart-coupon-form]");
        if (couponForm) {
            event.preventDefault();
            submitCouponForm(couponForm);
            return;
        }

        const form = event.target.closest("[data-cart-ajax-form]");
        if (!form) {
            return;
        }

        event.preventDefault();
        submitCartForm(form);
    });

    document.addEventListener("change", (event) => {
        const select = event.target.closest("[data-cart-coupon-select]");
        if (!select) {
            return;
        }

        const form = select.closest("[data-cart-coupon-form]");
        if (form) {
            submitCouponForm(form);
        }
    });

    document.addEventListener("show.bs.modal", (event) => {
        const modal = event.target.closest(".add-to-cart-modal");
        if (modal) {
            resetAddModal(modal);
        }
    });
})();
