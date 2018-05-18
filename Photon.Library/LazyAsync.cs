using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Photon.Library
{
    public class LazyAsync<T>
    {
        private readonly Lazy<Task<T>> lazyTask;


        public LazyAsync(Func<Task<T>> initFunc)
        {
            lazyTask = new Lazy<Task<T>>(() => Task.Run(initFunc));
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return lazyTask.Value.GetAwaiter();
        }
    }
}
