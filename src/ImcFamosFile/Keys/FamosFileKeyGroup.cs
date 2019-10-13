﻿using System;
using System.IO;

namespace ImcFamosFile
{
    public class FamosFileKeyGroup : FamosFileBase
    {
        #region Constructors

        public FamosFileKeyGroup()
        {
            //
        }

        internal FamosFileKeyGroup(BinaryReader reader) : base(reader)
        {
            this.DeserializeKey(FamosFileKeyType.CK, expectedKeyVersion: 1, keySize =>
            {
                var unknown = this.DeserializeInt32();
                var keyGroupIsClosed = this.DeserializeInt32() == 1;

                if (!keyGroupIsClosed)
                    throw new FormatException($"The key group is not closed. This may be a hint to an interruption that occured while writing the file content to disk.");
            });
        }

        #endregion

        #region Properties

        protected override FamosFileKeyType KeyType => FamosFileKeyType.CK;

        #endregion

        #region Methods

        internal override void Serialize(StreamWriter writer)
        {
            var data = new object[]
            {
                1,
                0
            };

            this.SerializeKey(writer, 1, data);
        }

        #endregion
    }
}
