using HealthCareAI.Application.Interfaces;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IDbConnection Connection => _connection ??= _connectionFactory.CreateConnection();
    public IDbTransaction? Transaction => _transaction;

    public async Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _connection = await _connectionFactory.CreateConnectionAsync();
            _transaction = _connection.BeginTransaction();
        }
    }

    public async Task CommitAsync()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            DisposeTransaction();
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            DisposeTransaction();
        }
    }

    private void DisposeTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
        _connection?.Dispose();
        _connection = null;
    }

    public async Task<int> SaveChangesAsync()
    {
        // For Dapper, we typically handle transactions explicitly
        // This method is kept for interface compatibility but doesn't do much
        return 1;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            DisposeTransaction();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
} 