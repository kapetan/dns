using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DNS.Protocol.Utils;

namespace DNS.Tests.Utils {

    public class TaskExtensionsTest {
        [Fact]
        public async Task WithoutCancellation() {
            object obj = new object();
            CancellationTokenSource cts = new CancellationTokenSource();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task<object> resultTask = tcs.Task.WithCancellation(cts.Token);

            tcs.SetResult(obj);
            object result = await resultTask;

            Assert.Same(obj, result);
        }

        [Fact]
        public async Task WithCancellation() {
            CancellationTokenSource cts = new CancellationTokenSource();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task resultTask = tcs.Task.WithCancellation(cts.Token);

            cts.Cancel();

            OperationCanceledException e =
                await Assert.ThrowsAsync<OperationCanceledException>(() => resultTask);
            Assert.Equal(cts.Token, e.CancellationToken);
        }

        [Fact]
        public async Task WithoutCancellationTimeout() {
            object obj = new object();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task<object> resultTask = tcs.Task.WithCancellationTimeout(TimeSpan.FromMilliseconds(60000));

            tcs.SetResult(obj);
            object result = await resultTask;

            Assert.Same(obj, result);
        }

        [Fact(Timeout = 30000)]
        public async Task WithoutCancellationTimeoutAndCancellationToken() {
            CancellationTokenSource cts = new CancellationTokenSource();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task<object> resultTask = tcs.Task.WithCancellationTimeout(TimeSpan.FromMilliseconds(60000), cts.Token);

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() => resultTask);
        }

        [Fact]
        public async Task WithCancellationTimeout() {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            Task resultTask = tcs.Task.WithCancellationTimeout(TimeSpan.FromMilliseconds(100));

            await Assert.ThrowsAsync<OperationCanceledException>(() => resultTask);
        }
    }
}
