﻿namespace ImcFamosFile;

/// <summary>
/// Describes the start time of measurement.
/// </summary>
public class FamosFileTriggerTime : FamosFileBase
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FamosFileTriggerTime"/> class.
    /// </summary>
    public FamosFileTriggerTime()
    {
        //
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FamosFileTriggerTime"/> class.
    /// </summary>
    /// <param name="dateTime">The trigger time.</param>
    /// <param name="timeMode">The time mode.</param>
    public FamosFileTriggerTime(DateTime dateTime, FamosFileTimeMode timeMode)
    {
        DateTime = dateTime;
        TimeMode = timeMode;
    }

    internal FamosFileTriggerTime(BinaryReader reader) : base(reader)
    {
        DateTime triggerTime = default;
        FamosFileTimeMode timeMode = FamosFileTimeMode.Unknown;

        var keyVersion = DeserializeInt32();

        DeserializeKey(keySize =>
        {
            // day
            var day = DeserializeInt32();

            if (!(1 <= day && day <= 31))
                throw new FormatException($"Expected value for 'day' property: '1..31'. Got {day}.");

            // month
            var month = DeserializeInt32();

            if (!(1 <= month && month <= 12))
                throw new FormatException($"Expected value for 'month' property: '1..12'. Got {month}.");

            // year
            var year = DeserializeInt32();

            if (year < 1980)
                throw new FormatException($"Expected value for 'year' property: >= '1980'. Got {year}.");

            // hour
            var hour = DeserializeInt32();

            if (!(0 <= hour && hour <= 23))
                throw new FormatException($"Expected value for 'hour' property: '0..23'. Got {hour}.");

            // minute
            var minute = DeserializeInt32();

            if (!(0 <= minute && minute <= 59))
                throw new FormatException($"Expected value for 'minute' property: '0..59'. Got {minute}.");

            // second
            var second = DeserializeReal();

            if (!(0 <= second && second <= 60))
                throw new FormatException($"Expected value for 'day' property: '0.0..60.0'. Got {second}.");

            // millisecond
            var millisecond = (int)((second - Math.Truncate(second)) * 1000);
            var intSecond = (int)Math.Truncate(second);

            // parse
            if (keyVersion == 1)
            {
                triggerTime = new DateTime(year, month, day, hour, minute, intSecond, millisecond, DateTimeKind.Unspecified);
            }
            else if (keyVersion == 2)
            {
                var timeZone = DeserializeInt32();

                timeMode = (FamosFileTimeMode)DeserializeInt32();
                triggerTime = new DateTimeOffset(year, month, day, hour, minute, intSecond, millisecond, TimeSpan.FromMinutes(timeZone)).UtcDateTime;
            }
            else
            {
                throw new FormatException($"Expected key version '1' or '2', got '{keyVersion}'.");
            }
        });

        DateTime = triggerTime;
        TimeMode = timeMode;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the trigger time.
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Gets or sets the time mode.
    /// </summary>
    public FamosFileTimeMode TimeMode { get; set; }

    private protected override FamosFileKeyType KeyType => FamosFileKeyType.NT;

    #endregion

    #region Methods

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        var other = (FamosFileTriggerTime)obj;

        if (other == null)
            return false;

        return DateTime.Equals(other.DateTime)
            && TimeMode.Equals(other.TimeMode);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(DateTime, TimeMode);
    }

    internal FamosFileTriggerTime Clone()
    {
        return (FamosFileTriggerTime)MemberwiseClone();
    }

    #endregion

    #region Serialization

    internal override void Serialize(BinaryWriter writer)
    {
        var data = new object[]
        {
            DateTime.Day,
            DateTime.Month,
            DateTime.Year,
            DateTime.Hour,
            DateTime.Minute,
            (decimal)DateTime.Second + DateTime.Millisecond / 1000,
            0, // since it is UTC+0 now
            0  // since it is UTC+0 now
        };

        SerializeKey(writer, 2, data);
    }

    #endregion
}
