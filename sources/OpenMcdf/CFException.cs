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

    /// <summary>
    /// Raised when a method call is invalid for the current object state
    /// </summary>
    [Serializable]
    public class CFInvalidOperation : CFException
    {
        public CFInvalidOperation()
           : base()
        {
        }

        protected CFInvalidOperation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CFInvalidOperation(string message)
            : base(message, null)
        {

        }

        public CFInvalidOperation(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }

    /// <summary>
    /// Raised when trying to add a duplicated CFItem
    /// </summary>
    /// <remarks>
    /// Items are compared by name as indicated by specs.
    /// Two items with the same name CANNOT be added within 
    /// the same storage or sub-storage. 
    /// </remarks>
    [Serializable]
    public class CFDuplicatedItemException : CFException
    {
        public CFDuplicatedItemException()
            : base()
        {
        }

        protected CFDuplicatedItemException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CFDuplicatedItemException(string message)
            : base(message, null)
        {

        }

        public CFDuplicatedItemException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }

    /// <summary>
    /// Raised when trying to load a Compound File with invalid, corrupted or mismatched fields (4.1 - specifications) 
    /// </summary>
    /// <remarks>
    /// This exception is NOT raised when Compound file has been opened with NO_VALIDATION_EXCEPTION option.
    /// </remarks>
    [Serializable]
    public class CFCorruptedFileException : CFException
    {
        public CFCorruptedFileException()
            : base()
        {
        }

        protected CFCorruptedFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CFCorruptedFileException(string message)
            : base(message, null)
        {

        }

        public CFCorruptedFileException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

    }

}
