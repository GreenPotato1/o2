using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O2.ToolKit.Core
{
    /// <summary>
    /// Class storage is configured to save states
    /// </summary>
    public static class O2Store

    {
        /// <summary>
        /// There are stored all the objects that are involved in the work
        /// </summary>
        private static readonly Dictionary<Type, object> StoreItemsDictionary;

        static O2Store()
        {
            StoreItemsDictionary = new Dictionary<Type, object>();
        }


        /// <summary>
        /// Create an object in the collection or store an existing one
        /// </summary>
        /// <typeparam name="TClass"> </typeparam>
        /// <param name="args"> </param>
        /// <returns> </returns>
        public static TClass CreateOrGet<TClass>(params object[] args) where TClass : class
        {
            var typeClass = typeof(TClass);
            if (StoreItemsDictionary.ContainsKey(typeClass)) return (TClass) StoreItemsDictionary[typeClass];
            var itemClass = (TClass) Activator.CreateInstance(typeClass, args);
            StoreItemsDictionary.Add(typeClass, itemClass);
            return (TClass) StoreItemsDictionary[typeClass];
        }

        public static async Task SaveState()
        {
            await Task.Delay(100);
            //Debug.WriteLine("Save state");
            //StoreItemsDictionary
            //   .Where(p => Attribute.IsDefined(p.Key, typeof(DataContractAttribute)))
            //   .Select(p => p.Value).ToList()
            //   .ForEach(i => i.SerializeDataContract());
        }
    }
}