using System;

namespace DefaultNamespace
{
    public class GenerationException: Exception
    {
        public GenerationException(string message) : base(message)
        {
        }
    }
}