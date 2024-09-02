/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * 
 * The Original Code is OpenMCDF - Compound Document Format library.
 * 
 * The Initial Developer of the Original Code is Federico Blaseotto.*/

using System;
using System.Runtime.Serialization;

namespace OpenMcdf
{
    /// <summary>
    /// OpenMCDF base exception.
    /// </summary>
    [Serializable]
    public class CFException : Exception
    {
        public CFException()
            : base()
        {
        }

        protected CFException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CFException(string message)
            : base(message, null)
        {

        }

        public CFException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }

    /// <summary>
    /// Raised when opening a file with invalid header
    /// or not supported COM/OLE Structured storage version.
    /// </summary>
    [Serializable]
    public class FileFormatException : FormatException
    {
        public FileFormatException()
            : base()
        {
        }

        protected FileFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public FileFormatException(string message)
            : base(message, null)
        {

        }

        public FileFormatException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }
}
