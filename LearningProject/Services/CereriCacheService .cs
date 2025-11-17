using LearningProject.Models.ViewModels;
using LearningProject.Services.Impl;

namespace LearningProject.Services
{
    public class CereriCacheService : ICereriCacheService
    {
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        private ViewModelPaginatedListCereri? _cachedData;
        private DateTime _lastLoadTime = DateTime.MinValue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CereriCacheService> _logger;

        public CereriCacheService(
            IServiceScopeFactory scopeFactory,
            ILogger<CereriCacheService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<ViewModelPaginatedListCereri> GetCachedDataAsync(
            string sortOrder,
            int pageNumber = 1,
            int pageSize = 3,
            string? searchString = null,
            string? filter = "toate",
            double? nrCrt = null,
            string? description = null,
            string? creat_de = null,
            string? sters_de = null,
            DateTime? data_creare = null,
            DateTime? data_stergere = null)
        {
            // Check if cache needs reload (first time or expired)
            if (_cachedData == null || (DateTime.UtcNow - _lastLoadTime).TotalMinutes >= 30)
            {
                await ReloadCacheAsync();
            }

            // Return cached data (with filters applied if needed)
            // Note: You might want to apply filters on the cached data here
            return _cachedData!;
        }

        public async Task ReloadCacheAsync()
        {
            await _cacheLock.WaitAsync();
            try
            {
                _logger.LogInformation("Reloading cache at {Time}", DateTime.UtcNow);

                // Create a scope to resolve scoped dependencies
                using var scope = _scopeFactory.CreateScope();
                var yourService = scope.ServiceProvider.GetRequiredService<IYourCereriService>();//edit here

                // Load fresh data
                //edit here
                _cachedData = await yourService.GetFilteredCereriAsync(
                    sortOrder: "id",
                    pageNumber: 1,
                    pageSize: 100 // Adjust as needed
                                  // Add other default parameters
                );

                _lastLoadTime = DateTime.UtcNow;
                _logger.LogInformation("Cache reloaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading cache");
                throw;
            }
            finally
            {
                _cacheLock.Release();
            }
        }
    }
}
