using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.Base.Converters
{
    public abstract class BtbrdCoreIdConverter
    {
        public Type IdType { get; private set; }

        internal BtbrdCoreIdConverter(Type idType)
        {
            IdType = idType;
        }

        public abstract string ConvertToString(object obj);
        public abstract object ConvertFromString(string obj);
    }
}
