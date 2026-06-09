const adminAuth = (() => {
    const csrfEndpoint = "/api/security/csrf-token";
    const loginEndpoint = "/api/admin/auth/login";
    const meEndpoint = "/api/admin/auth/me";
    const logoutEndpoint = "/api/admin/auth/logout";

    function normalizeApiResponse(payload) {
        return {
            succeeded: payload?.succeeded ?? payload?.Succeeded ?? false,
            data: payload?.data ?? payload?.Data ?? null,
            error: payload?.error ?? payload?.Error ?? null
        };
    }

    function errorMessage(error, fallback) {
        return error?.message ?? error?.Message ?? fallback;
    }

    async function requestCsrfToken() {
        const response = await fetch(csrfEndpoint, {
            method: "GET",
            credentials: "include"
        });

        const payload = normalizeApiResponse(await response.json());
        if (!response.ok || !payload.succeeded || !payload.data?.token) {
            throw new Error(errorMessage(payload.error, "無法取得安全權杖。"));
        }

        return {
            headerName: payload.data.headerName ?? payload.data.HeaderName ?? "X-CSRF-TOKEN",
            token: payload.data.token ?? payload.data.Token
        };
    }

    async function login({ account, password, rememberMe }) {
        const csrf = await requestCsrfToken();

        const response = await fetch(loginEndpoint, {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json",
                [csrf.headerName]: csrf.token
            },
            body: JSON.stringify({ account, password, rememberMe })
        });

        const payload = normalizeApiResponse(await response.json());
        if (!response.ok || !payload.succeeded) {
            throw new Error(errorMessage(payload.error, "登入失敗，請確認帳號密碼。"));
        }

        return payload.data;
    }

    async function getCurrentUser() {
        const response = await fetch(meEndpoint, {
            method: "GET",
            credentials: "include"
        });

        if (response.status === 401 || response.status === 403) {
            return null;
        }

        const payload = normalizeApiResponse(await response.json());
        if (!response.ok || !payload.succeeded) {
            throw new Error(errorMessage(payload.error, "無法取得登入狀態。"));
        }

        return payload.data;
    }

    async function requireAdmin() {
        const user = await getCurrentUser();
        if (!user) {
            window.location.replace("/admin/login");
            return null;
        }

        return user;
    }

    async function logout() {
        const csrf = await requestCsrfToken();

        await fetch(logoutEndpoint, {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json",
                [csrf.headerName]: csrf.token
            },
            body: "{}"
        });

        window.location.replace("/admin/login");
    }

    return {
        login,
        logout,
        requireAdmin,
        getCurrentUser
    };
})();
