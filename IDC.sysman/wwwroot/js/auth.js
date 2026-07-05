window.logoutAndRedirect = function (idToken) {
    localStorage.clear();
    sessionStorage.clear();

    var cfg = (window.appConfig && window.appConfig.keycloak) || {};
    var keycloakUrl = cfg.url || "";
    var realm = cfg.realm || "";
    var clientId = cfg.clientId || "";
    var postLogoutRedirectUri = cfg.postLogoutRedirectUri || (window.location.origin + "/");

    var url = keycloakUrl + "/realms/" + realm + "/protocol/openid-connect/logout" +
        "?client_id=" + encodeURIComponent(clientId) +
        "&post_logout_redirect_uri=" + encodeURIComponent(postLogoutRedirectUri) +
        (idToken ? "&id_token_hint=" + encodeURIComponent(idToken) : "");

    window.location.href = url;
};