using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.Base.Converters
{
    public sealed class BtbrdCoreIdConverter<T> : BtbrdCoreIdConverter
    {
        private Func<T, string> ToStringConverter { get; set; }
        private Func<string, T> ToIdConverter { get; set; }

        public BtbrdCoreIdConverter(Func<T, string> toStringConverter, Func<string, T> toIdConverter) : base(typeof(T))
        {
            ToStringConverter = toStringConverter ?? throw new ArgumentNullException("toStringConverter", "Converter must not be null.");
            ToIdConverter = toIdConverter ?? throw new ArgumentNullException("toIdConverter", "Converter must not be null.");
        }

        public override string ConvertToString(object id)
        {
            if (!(id is T))
            {
                var msg = string.Format("Id is not of the type {0}.", this.IdType.FullName);
                throw new ArgumentException(msg, "id");
            }

            // Can throw exception, it's ok.
            return this.ToStringConverter.Invoke((T)id);
        }

        public override object ConvertFromString(string idString)
        {

            // Can throw exception, it's ok.
            return this.ToIdConverter.Invoke(idString);
        }
    }
}
