window.logoutAndRedirect = function (idToken) {
    localStorage.clear();
    sessionStorage.clear();

    var url = "http://192.168.93.198:8080/realms/TestSSO/protocol/openid-connect/logout" +
        "?client_id=TestSSO" +
        "&post_logout_redirect_uri=" + encodeURIComponent("http://localhost:5105/") +
        (idToken ? "&id_token_hint=" + encodeURIComponent(idToken) : "");

    window.location.href = url;
};