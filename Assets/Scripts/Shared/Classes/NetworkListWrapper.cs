using System;
using System.Collections.Generic;

namespace Shared.Classes
{
    [Serializable]
    public class NetworkListWrapper<T>
    {
        public List<T> value;

        public NetworkListWrapper(T data)
        {
            value = new List<T> { data };
        }

        public NetworkListWrapper(List<T> data)
        {
            value = data;
        }
    }
}