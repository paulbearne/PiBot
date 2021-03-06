﻿namespace UWPBiped
{
    using System;

    /// <summary>
    ///     A finalization callback.
    /// </summary>
    /// <param name="data">
    ///     The external data that was passed in when creating the object being finalized.
    /// </param>
    delegate void JavaScriptObjectFinalizeCallback(IntPtr data);
}
