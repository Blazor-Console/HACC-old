using Spectre.Console;

namespace HACC.Spectre;

internal sealed class DefaultExclusivityMode : IExclusivityMode
{
    private readonly SemaphoreSlim _semaphore;

    public DefaultExclusivityMode()
    {
        this._semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
    }

    public T Run<T>(Func<T> func)
    {
        // Try acquiring the exclusivity semaphore
        if (!this._semaphore.Wait(millisecondsTimeout: 0)) throw CreateExclusivityException();

        try
        {
            return func();
        }
        finally
        {
            this._semaphore.Release(releaseCount: 1);
        }
    }

    public async Task<T> Run<T>(Func<Task<T>> func)
    {
        // Try acquiring the exclusivity semaphore
        if (!await this._semaphore.WaitAsync(millisecondsTimeout: 0).ConfigureAwait(continueOnCapturedContext: false))
            throw CreateExclusivityException();

        try
        {
            return await func().ConfigureAwait(continueOnCapturedContext: false);
        }
        finally
        {
            this._semaphore.Release(releaseCount: 1);
        }
    }

    private static Exception CreateExclusivityException()
    {
        return new InvalidOperationException(
            message: "Trying to run one or more interactive functions concurrently. " +
                     "Operations with dynamic displays (e.g. a prompt and a progress display) " +
                     "cannot be running at the same time.");
    }
}