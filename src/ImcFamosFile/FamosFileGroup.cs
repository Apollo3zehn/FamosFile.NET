﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ImcFamosFile
{
    public class FamosFileGroup : FamosFileBaseExtended
    {
        #region Fields

        private int _index;

        #endregion

        #region Constructors

        public FamosFileGroup()
        {
            //
        }

        internal FamosFileGroup(BinaryReader reader, int codePage) : base(reader, codePage)
        {
            this.DeserializeKey(expectedKeyVersion: 1, keySize =>
            {
                this.Index = this.DeserializeInt32();

                this.Name = this.DeserializeString();
                this.Comment = this.DeserializeString();
            });
        }

        #endregion

        #region Properties

        internal int Index
        {
            get { return _index; }
            set
            {
                if (value <= 0)
                    throw new FormatException($"Expected index > '0', got '{value}'.");

                _index = value;
            }
        }

        public string Name { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<FamosFileText> Texts { get; private set; } = new List<FamosFileText>();
        public List<FamosFileSingleValue> SingleValues { get; private set; } = new List<FamosFileSingleValue>();
        public List<FamosFileChannelInfo> ChannelInfos { get; private set; } = new List<FamosFileChannelInfo>();

        #endregion

        #region Serialization

        internal override void Serialize(StreamWriter writer)
        {
            var data = string.Join(',', new object[]
            {
                this.Index,
                this.Name.Length, this.Name,
                this.Comment.Length, this.Comment
            });

            this.SerializeKey(writer, FamosFileKeyType.CB, 1, data);
#warning TODO: Enforce static (?) KeyType { get } property on all classes that implement FamosFileBase.
        }

        #endregion
    }
}
