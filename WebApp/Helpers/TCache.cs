using System;

namespace WebApp
{
    public class TCache<T>
    {
        public T Get(string cacheKeyName, int cacheTimeOutSeconds, Func<T> func)
        {
            return new TCacheInternal<T>().Get(
                cacheKeyName, cacheTimeOutSeconds, func);
        }
    }
}