function initTheme() {
    const savedTheme = localStorage.getItem("daynight-theme");
    const theme = savedTheme === "carbon" ? "carbon" : "snow";

    document.documentElement.classList.toggle("carbon", theme === "carbon");
    document.body.classList.toggle("carbon", theme === "carbon");
    updateThemeButtons(theme);
}

function setTheme(theme) {
    const normalizedTheme = theme === "carbon" ? "carbon" : "snow";

    document.documentElement.classList.toggle("carbon", normalizedTheme === "carbon");
    document.body.classList.toggle("carbon", normalizedTheme === "carbon");
    localStorage.setItem("daynight-theme", normalizedTheme);
    updateThemeButtons(normalizedTheme);
}

function updateThemeButtons(theme) {
    document.querySelectorAll("[data-theme-button]").forEach((button) => {
        button.classList.toggle("active", button.dataset.themeButton === theme);
    });

    document.querySelectorAll(".theme-btn-snow").forEach((button) => {
        button.classList.toggle("active", theme === "snow");
    });

    document.querySelectorAll(".theme-btn-carbon").forEach((button) => {
        button.classList.toggle("active", theme === "carbon");
    });
}

function getGreeting() {
    const hour = new Date().getHours();

    if (hour < 12) {
        return "早安";
    }

    if (hour < 17) {
        return "午安";
    }

    return "晚安";
}

function setGreeting() {
    const greetingElement = document.getElementById("greeting");

    if (!greetingElement) {
        return;
    }

    const displayName = greetingElement.dataset.displayName;
    greetingElement.textContent = displayName
        ? `${getGreeting()}，${displayName}`
        : getGreeting();
}

function setDateRange(range, button) {
    document.querySelectorAll(".date-btn").forEach((dateButton) => {
        dateButton.classList.remove("active");
    });

    button.classList.add("active");
    document.dispatchEvent(new CustomEvent("daynight:date-range", { detail: { range } }));
}

function selectMessage(element, index) {
    document.querySelectorAll(".message-item").forEach((item) => {
        item.classList.remove("active");
    });

    element.classList.add("active");
    element.classList.remove("unread");
    updateMessageView(index);
}

function updateMessageView(index) {
    const messages = [
        {
            subject: "客服訊息待處理",
            sender: "客服中心",
            email: "support@example.local",
            date: "待正式資料來源",
            paragraphs: [
                "此區塊應由後台訊息服務提供資料。",
                "訊息內容會以 textContent 輸出，避免插入未清理的動態 HTML。"
            ]
        },
        {
            subject: "系統通知",
            sender: "系統",
            email: "system@example.local",
            date: "待正式資料來源",
            paragraphs: [
                "正式通知資料應由後端 API 提供。",
                "若未來需要富文字內容，必須先建立白名單清理機制。"
            ]
        }
    ];

    const message = messages[index] || messages[0];
    const subjectElement = document.querySelector(".message-view-subject");
    const senderElement = document.querySelector(".message-view-sender-name");
    const emailElement = document.querySelector(".message-view-sender-email");
    const dateElement = document.querySelector(".message-view-date");
    const bodyElement = document.querySelector(".message-view-body");

    if (subjectElement) {
        subjectElement.textContent = message.subject;
    }

    if (senderElement) {
        senderElement.textContent = message.sender;
    }

    if (emailElement) {
        emailElement.textContent = message.email;
    }

    if (dateElement) {
        dateElement.textContent = message.date;
    }

    if (bodyElement) {
        const paragraphs = message.paragraphs.map((text) => {
            const paragraph = document.createElement("p");
            paragraph.textContent = text;
            return paragraph;
        });

        bodyElement.replaceChildren(...paragraphs);
    }
}

function initKanban() {
    const cards = document.querySelectorAll(".kanban-card");
    const columns = document.querySelectorAll(".kanban-cards");

    cards.forEach((card) => {
        card.setAttribute("draggable", "true");

        card.addEventListener("dragstart", () => {
            card.classList.add("dragging");
        });

        card.addEventListener("dragend", () => {
            card.classList.remove("dragging");
        });
    });

    columns.forEach((column) => {
        column.addEventListener("dragover", (event) => {
            event.preventDefault();

            const dragging = document.querySelector(".dragging");
            if (dragging) {
                column.appendChild(dragging);
            }
        });
    });
}

function initToggles() {
    document.querySelectorAll(".toggle input").forEach((toggle) => {
        toggle.setAttribute("aria-checked", toggle.checked ? "true" : "false");
        toggle.addEventListener("change", function () {
            this.setAttribute("aria-checked", this.checked ? "true" : "false");
        });
    });
}

function toggleMobileMenu() {
    const menu = document.querySelector(".mobile-menu");
    const overlay = document.querySelector(".mobile-menu-overlay");

    if (!menu || !overlay) {
        return;
    }

    const isActive = !menu.classList.contains("active");
    menu.classList.toggle("active", isActive);
    overlay.classList.toggle("active", isActive);
    document.body.style.overflow = isActive ? "hidden" : "";
}

function closeMobileMenu() {
    const menu = document.querySelector(".mobile-menu");
    const overlay = document.querySelector(".mobile-menu-overlay");

    if (!menu || !overlay) {
        return;
    }

    menu.classList.remove("active");
    overlay.classList.remove("active");
    document.body.style.overflow = "";
}

document.addEventListener("DOMContentLoaded", () => {
    initTheme();
    setGreeting();

    if (document.querySelector(".kanban-board")) {
        initKanban();
    }

    if (document.querySelector(".toggle")) {
        initToggles();
    }

    const overlay = document.querySelector(".mobile-menu-overlay");
    if (overlay) {
        overlay.addEventListener("click", closeMobileMenu);
    }
});
