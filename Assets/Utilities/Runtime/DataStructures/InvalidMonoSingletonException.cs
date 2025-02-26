using System;

namespace CustomUtilities.DataStructures
{
    public class InvalidMonoSingletonException: Exception
    {
        public override string Message { get; }

        public InvalidMonoSingletonException(string massage)
        {
            Message = massage;
        }
    }
}