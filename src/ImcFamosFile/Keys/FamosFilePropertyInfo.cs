﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ImcFamosFile
{
    public class FamosFilePropertyInfo : FamosFileBaseExtended
    {
        #region Constructors

        public FamosFilePropertyInfo()
        {
            //
        }

        internal FamosFilePropertyInfo(BinaryReader reader, int codePage) : base(reader, codePage)
        {
            this.DeserializeKey(expectedKeyVersion: 1, keySize =>
            {
                var startPosition = this.Reader.BaseStream.Position;

                while (this.Reader.BaseStream.Position - startPosition < keySize)
                {
                    var property = this.DeserializeProperty();
                    this.Properties.Add(property);
                }
            });
        }

        #endregion

        #region Properties

        public List<FamosFileProperty> Properties { get; } = new List<FamosFileProperty>();
        protected override FamosFileKeyType KeyType => FamosFileKeyType.Np;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Regex MatchProperty { get; } = new Regex("\"(.*? )\"\\s\"(.*?)\"\\s([0-9])\\s([0-9])");

        #endregion

        #region Serialization

        internal override void Serialize(BinaryWriter writer)
        {
            var propertyData = new List<object>();

            foreach (var property in this.Properties)
            {
                propertyData.AddRange(property.GetPropertyData());
            }

            this.SerializeKey(writer, 1, propertyData.ToArray());
        }

        private FamosFileProperty DeserializeProperty()
        {
            var bytes = this.DeserializeKeyPart();
            var rawValue = Encoding.GetEncoding(this.CodePage).GetString(bytes);
            var result = this.MatchProperty.Match(rawValue);

            var name = result.Groups[0].Value;
            var value = result.Groups[0].Value;
            var type = (FamosFilePropertyType)int.Parse(result.Groups[0].Value);
            var flags = (FamosFilePropertyFlags)int.Parse(result.Groups[0].Value);

            return new FamosFileProperty(name, value, type, flags);
        }

        #endregion
    }
}