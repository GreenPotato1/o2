using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    public sealed class ValueHolder<T>
    {
        [CanBeNull] public T Instance;

        public ValueHolder(T instance = default(T))
        {
            Instance = instance;
        }

        public override string ToString()
        {
            return $"{Instance}";
        }
    }
}