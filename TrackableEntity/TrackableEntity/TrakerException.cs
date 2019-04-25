using System;

namespace TrackableEntity
{
    /// <summary>
    /// Ошибки логики уровня трекера
    /// </summary>
    [Serializable]
    class TrakerException : Exception
    {
        public TrakerException(string message) : base(message)
        {
        }
    }
}
