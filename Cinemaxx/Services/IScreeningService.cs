namespace Cinemaxx.Services;

public interface IScreeningService
{
    Task<bool> HasOverlapAsync(int hallId, DateTime startTime, DateTime endTime, int? ignoreScreeningId = null);
}
