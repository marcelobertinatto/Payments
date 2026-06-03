namespace BuildingBlocks.Middleware.Exceptions
{
    public class AppException : System.Exception
    {
        public int StatusCode { get; }

        public AppException(string message,int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public AppException(string message) : base(message)
        {
        }
    }
}
