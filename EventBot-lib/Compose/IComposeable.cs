using System;
using System.Collections.Generic;

namespace EventBot.Compose {
    public interface IComposeable<T> {
        public List<T> Items { get; protected set; }

        public void Populate(Action<T> configuration);

    }

    public static class IComposeableExtensions {

        public static T Add<T, U>(this IComposeable<T> c) where U : T, new() {
            return c.Add<T, U>(t => { });
        }
        
        public static U Add<T, U>(this IComposeable<T> c, Action<T> configuration) where U : T, new() {
            U item = new();
            c.Items.Add(item);
            configuration(item);
            if(item is IComposeable<T> sub)
                sub.Populate(configuration);
            return item;
        }
    }
    
}