using System.Net;
using System.Text.Json;

namespace APBD_s31722_9_APi_2.Exceptions;

public class BadRequestException : Exception
{
    public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public BadRequestException(string message) : base(message) { }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

[Serializable]
public class InternalServerErrorException : Exception
{
    public HttpStatusCode StatusCode => HttpStatusCode.InternalServerError;
    public InternalServerErrorException(string message) : base(message) { }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

