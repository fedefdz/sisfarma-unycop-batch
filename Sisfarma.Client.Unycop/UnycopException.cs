using System;
using System.Runtime.Serialization;

namespace Sisfarma.Client.Unycop
{
    public class UnycopException : Exception
    {
        public UnycopException()
        { }

        public UnycopException(string message) : base(message)
        { }

        public UnycopException(string message, Exception innerException) : base(message, innerException)
        { }

        protected UnycopException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }

    public class UnycopFailResponseException : UnycopException
    {
        public string Codigo { get; }

        public string Descripcion { get; }

        public override string Message => $"Unycop Rest Client no pudo resolver una operación: {Codigo}-{Descripcion}";

        public UnycopFailResponseException(string codigo, string descripcion)
        {
            Codigo = codigo;
            Descripcion = descripcion;
        }
    }
}