function initTheme() {
    const savedTheme = localStorage.getItem("mvc-shop-admin-theme");
    if (savedTheme === "carbon") {
        document.documentElement.classList.add("carbon");
        document.body.classList.add("carbon");
    }

    updateThemeButtons(savedTheme === "carbon" ? "carbon" : "snow");
}

function setTheme(theme) {
    const isCarbon = theme === "carbon";
    document.documentElement.classList.toggle("carbon", isCarbon);
    document.body.classList.toggle("carbon", isCarbon);
    localStorage.setItem("mvc-shop-admin-theme", isCarbon ? "carbon" : "snow");
    updateThemeButtons(isCarbon ? "carbon" : "snow");
}

function updateThemeButtons(theme) {
    document.querySelectorAll("[data-theme-button]").forEach((button) => {
        button.classList.toggle("active", button.dataset.themeButton === theme);
    });
}

document.addEventListener("DOMContentLoaded", initTheme);
