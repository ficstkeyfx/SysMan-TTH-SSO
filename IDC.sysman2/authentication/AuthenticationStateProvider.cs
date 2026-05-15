using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace QLNguoiDung.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ProtectedLocalStorage _localStorage;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage, ProtectedLocalStorage localStorage)
        {
            _sessionStorage = sessionStorage;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userIdSessionStorageResult = await _sessionStorage.GetAsync<string>("userId");
                var userIdLocalStorageResult = await _localStorage.GetAsync<string>("userId");
                string? userIdSession = userIdSessionStorageResult.Success ? userIdSessionStorageResult.Value : null;
                string? userIdLocal = userIdLocalStorageResult.Success ? userIdLocalStorageResult.Value : null;
                string? userId = userIdSession == null ? userIdLocal : userIdSession;

                var dateExpireSessionStorageResult = await _sessionStorage.GetAsync<DateTime>("dateExpire");
                var dateExpireLocalStorageResult = await _localStorage.GetAsync<DateTime>("dateExpire");
                DateTime? dateExpireSession = dateExpireSessionStorageResult.Success ? dateExpireSessionStorageResult.Value : null;
                DateTime? dateExpireLocal = dateExpireLocalStorageResult.Success ? dateExpireLocalStorageResult.Value : null;
                DateTime? dateExpire = dateExpireSession == null ? dateExpireLocal : dateExpireSession;

                if (userId == null || dateExpire == null)
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                if (dateExpire.Value < DateTime.Now)
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userId ?? ""),

            }, "CustomAuth"));
                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }

        }

        public async Task<string> getTypeStorage()
        {
            try
            {
                var userIdSessionStorageResult = await _sessionStorage.GetAsync<string>("userId");
                var userIdLocalStorageResult = await _localStorage.GetAsync<string>("userId");
                string? userIdSession = userIdSessionStorageResult.Success ? userIdSessionStorageResult.Value : null;
                string? userIdLocal = userIdLocalStorageResult.Success ? userIdLocalStorageResult.Value : null;

                return userIdSession == null ? "local" : "session";

            }
            catch
            {
                return "";
            }
        }
        public async Task UpdateAuthenticationState(string? userId, string typeStorage = "session", bool addNew = false)
        {
            ClaimsPrincipal claimsPrincipal;

            if (userId != null && typeStorage != "")
            {
                if (typeStorage == "session")
                {
                    await _sessionStorage.SetAsync("userId", userId);
                    if (!addNew)
                    {
                        await _sessionStorage.SetAsync("dateExpire", DateTime.Now.AddHours(3));
                    }
                }
                if (typeStorage == "local")
                {
                    await _localStorage.SetAsync("userId", userId);
                    if (!addNew)
                    {
                        await _localStorage.SetAsync("dateExpire", DateTime.Now.AddDays(7));
                    }

                }
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userId),
                }));
            }
            else
            {
                await _sessionStorage.DeleteAsync("userId");
                await _localStorage.DeleteAsync("userId");

                await _sessionStorage.DeleteAsync("dateExpire");
                await _localStorage.DeleteAsync("dateExpire");

                claimsPrincipal = _anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}