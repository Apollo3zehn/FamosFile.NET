﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImcFamosFile
{
    public class FamosFileDataField : FamosFileBaseExtended
    {
        #region Constructors

        public FamosFileDataField()
        {
            //
        }

        public FamosFileDataField(BinaryReader reader, int codePage) : base(reader, codePage)
        {
            FamosFileXAxisScaling? currentXAxisScaling = null;
            FamosFileZAxisScaling? currentZAxisScaling = null;
            FamosFileTriggerTimeInfo? currentTriggerTimeInfo = null;

            this.DeserializeKey(expectedKeyVersion: 1, keySize =>
            {
                var componentCount = this.DeserializeInt32();
                var type = (FamosFileDataFieldType)this.DeserializeInt32();
                var dimension = this.DeserializeInt32();

                this.Type = type;

                if (dimension != this.Dimension)
                    throw new FormatException($"Expected data field dimension '{this.Dimension}', got '{dimension}'.");

                if (this.Type == FamosFileDataFieldType.MultipleYToSingleEquidistantTime &&
                    this.Dimension != 1)
                    throw new FormatException($"The field dimension is invalid. Expected '1', got '{this.Dimension}'.");

                if (this.Type > FamosFileDataFieldType.MultipleYToSingleEquidistantTime &&
                    this.Dimension != 2)
                    throw new FormatException($"The field dimension is invalid. Expected '2', got '{this.Dimension}'.");
            });

            while (true)
            {
                if (this.Reader.BaseStream.Position >= this.Reader.BaseStream.Length)
                    return;

                var nextKeyType = this.DeserializeKeyType();

                if (nextKeyType == FamosFileKeyType.Unknown)
                {
                    this.SkipKey();
                    continue;
                }

                else if (nextKeyType == FamosFileKeyType.CD)
                    currentXAxisScaling = new FamosFileXAxisScaling(this.Reader, this.CodePage);

                else if (nextKeyType == FamosFileKeyType.CZ)
                    currentZAxisScaling = new FamosFileZAxisScaling(this.Reader, this.CodePage);

                else if (nextKeyType == FamosFileKeyType.NT)
                    currentTriggerTimeInfo = new FamosFileTriggerTimeInfo(this.Reader);

                else if (nextKeyType == FamosFileKeyType.CC)
                {
                    var component = new FamosFileComponent(this.Reader, this.CodePage, currentXAxisScaling, currentZAxisScaling, currentTriggerTimeInfo);
                    this.Components.Add(component);
                }

                else
                {
                    // go back to start of key
                    this.Reader.BaseStream.Position -= 4;
                    break;
                }
            }
        }

        #endregion

        #region Properties

        public FamosFileDataFieldType Type { get; set; } = FamosFileDataFieldType.MultipleYToSingleEquidistantTime;
        public List<FamosFileComponent> Components { get; } = new List<FamosFileComponent>();

        public int Dimension => this.Type == FamosFileDataFieldType.MultipleYToSingleEquidistantTime ? 1 : 2;

        #endregion

        #region Methods

        internal override void Validate()
        {
            if (this.Components.Count < this.Dimension)
                throw new FormatException($"Expected number of data field components is >= '{this.Dimension}', got '{this.Components.Count}'.");

            foreach (var component in this.Components)
            {
                component.Validate();
            }
        }

        #endregion

        #region Serialization

        internal override void BeforeSerialize()
        {
            foreach (var component in this.Components)
            {
                component.BeforeSerialize();
            }
        }

        internal override void Serialize(StreamWriter writer)
        {
            var data = string.Join(',', new object[]
            {
                this.Components.Count,
                (int)this.Type,
                this.Dimension
            });

            this.SerializeKey(writer, FamosFileKeyType.CG, 1, data);

            if (this.Components.Any())
            {
                var firstComponent = this.Components.First();

                // CD
                if (firstComponent.XAxisScaling != null)
                    firstComponent.XAxisScaling.Serialize(writer);

                // CZ
                if (firstComponent.ZAxisScaling != null)
                    firstComponent.ZAxisScaling.Serialize(writer);

                // NT
                if (firstComponent.TriggerTimeInfo != null)
                    firstComponent.TriggerTimeInfo.Serialize(writer);

                foreach (var component in this.Components)
                {
                    component.Serialize(writer);
                }
            }
        }

        #endregion

        #region Deserialization

        internal override void AfterDeserialize()
        {
            foreach (var component in this.Components)
            {
                component.AfterDeserialize();
            }
        }

        #endregion
    }
}
