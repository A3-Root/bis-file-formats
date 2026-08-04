namespace BIS.PAA.Encoder
{
    /// <summary>Specifies alpha-channel behavior for PAA encoding.</summary>
    public enum PAAFlags
    {
        /// <summary>Uses no optional encoding behavior.</summary>
        None = 0,

        /// <summary>
        /// Interpolated alpha channel (default behaviour)
        /// </summary>
        InterpolatedAlpha = 1,

        /// <summary>
        /// Alpha channel interpolation disabled
        /// </summary>
        KeepAlphaAsIs = 2
    }
}
