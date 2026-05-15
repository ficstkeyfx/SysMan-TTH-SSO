using SysMan.Models;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SysMan.Services.IApiKCServices
{
    public interface IApiKCServices
    {
        // ✅ CRUD OPERATIONS - Match exact implementation signatures
        Task<List<T>> GetAll<T>(string apiPath, string lamda = null);     // GET request
        Task<List<object>> GetByColumns(string apiPath, string columns = "", string lamda = null);
        Task<T> GetId<T>(string apiPath, string seqName); // GET request
        Task Update<T>(string apiPath, object data); // PUT request  
        Task Create<T>(string apiPath, object data); // POST request
        Task Delete<T>(string apiPath, string lamda = "", bool showMessage = true); // DELETE request
        Task Delete<T>(string apiPath, List<T> data); // DELETE request (bulk)
        
        // ✅ KEYCLOAK TOKEN MANAGEMENT - Match implementation method names
        Task<bool> RefreshKeycloakTokenAsync(); // Refresh Keycloak token (actual method name in implementation)
        void ClearToken(); // Clear cached token
        bool IsTokenValid(); // Check if token is valid
        
        // ✅ KEYCLOAK TOKEN ACCESS - For Login.razor and other components
        Task<string> GetCurrentKeycloakToken(); // Get current Keycloak token
        
        // ✅ DEBUG AND UTILITY METHODS - Match implementation
        void LogKeycloakTokenInfo(); // Log token information to console
        Task<bool> TestKeycloakConnection(); // Test Keycloak connection

        // ✅ INTERFACE COMPATIBILITY - Add alias for RefreshTokenAsync (if needed for backward compatibility)
        async Task<bool> RefreshTokenAsync() => await RefreshKeycloakTokenAsync(); // Alias for compatibility
    }
}