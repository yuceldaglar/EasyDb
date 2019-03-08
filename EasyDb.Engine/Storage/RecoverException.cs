using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Storage
{
    [global::System.Serializable]
    public class RecoverException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RecoverException() { }
        public RecoverException(string message) : base(message) { }
        public RecoverException(string message, Exception inner) : base(message, inner) { }
        protected RecoverException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
