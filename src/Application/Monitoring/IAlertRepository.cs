using Domain.Monitoring;

namespace Application.Monitoring;

public interface IAlertRepository
{
    Task AddAsync(Alert alert, CancellationToken ct = default);

    Task<IReadOnlyList<Alert>> GetAllAsync(CancellationToken ct = default);
}
