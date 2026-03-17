using TqkLibrary.Scrcpy.Attributes;

namespace TqkLibrary.Scrcpy.Enums
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum Orientations : int
    {
        Auto = -1,
        [OptionName("0")]
        Natural = 0,
        /// <summary>90° counterclockwise</summary>
        [OptionName("90")]
        Counterclockwise90 = 1,
        /// <summary>180°</summary>
        [OptionName("180")]
        Flip = 2,
        /// <summary>90° clockwise (270° counterclockwise)</summary>
        [OptionName("270")]
        Clockwise90 = 3,
        /// <summary>0° and lock orientation</summary>
        [OptionName("0@")]
        Natural_Locked = 10,
        /// <summary>90° counterclockwise and lock orientation</summary>
        [OptionName("90@")]
        Counterclockwise90_Locked = 11,
        /// <summary>180° and lock orientation</summary>
        [OptionName("180@")]
        Flip_Locked = 12,
        /// <summary>90° clockwise and lock orientation</summary>
        [OptionName("270@")]
        Clockwise90_Locked = 13,
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
