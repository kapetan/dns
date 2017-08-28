using System;
using System.Threading;
using System.Threading.Tasks;

namespace DNS.Protocol.Utils {
    public static class TaskExtensions {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken token) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CancellationTokenRegistration registration = token.Register(src => {
                ((TaskCompletionSource<bool>) src).TrySetResult(true);
            }, tcs);

            using(registration) {
                if(await Task.WhenAny(task, tcs.Task) != task) {
                    throw new OperationCanceledException(token);
                }
            }

            return await task;
        }

        public static Task<T> WithCancellationOrTimeout<T>(this Task<T> task, CancellationToken cancellationToken,
            TimeSpan timeout)
        {
            var cancellationTokenSource = new CancellationTokenSource(timeout);
            var mergedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
            return task.WithCancellation(mergedSource.Token);
        }

        public static async Task<T> WithCancellationTimeout<T>(this Task<T> task, int timeout) {
            using(CancellationTokenSource cts = new CancellationTokenSource(timeout)) {
                return await task.WithCancellation(cts.Token);
            }
        }
    }
}
