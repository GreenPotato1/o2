using System;
using System.Reflection;

namespace O2.Black.Toolkit.Core
{
    public class Singleton<T> where T : class
    {
        private static T _instance;

        protected Singleton()
        {
        }
        
        private static T CreateInstance()
        {            
            var cInfo = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[0],
                new ParameterModifier[0]);

            return (T)cInfo.Invoke(null);
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance();
                }

                return _instance;
            }
        }
    }
}