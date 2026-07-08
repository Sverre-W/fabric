namespace Fabric.Hardware.Desfire.Protocol;

public static class DesfireStatusCodeExtensions
{
    public static bool IsSuccess(this DesfireStatusCode statusCode)
    {
        return statusCode is DesfireStatusCode.Success or DesfireStatusCode.SuccessLimitedFunctionality;
    }

    public static string Describe(this DesfireStatusCode statusCode)
    {
        return statusCode switch
        {
            DesfireStatusCode.CommandAborted => "Command aborted by the card. Check the selected application, access rights, key material, and payload size.",
            DesfireStatusCode.AuthenticationError => "Authentication failed. The key or key version likely does not match the card.",
            DesfireStatusCode.PermissionDenied => "Permission denied. The authenticated key does not grant access to this command.",
            DesfireStatusCode.ParameterError => "The card rejected one or more command parameters.",
            DesfireStatusCode.LengthError => "The command payload length is invalid for this file or operation.",
            DesfireStatusCode.ApplicationNotFound => "The selected application could not be found on the card.",
            _ => statusCode.ToString(),
        };
    }
}
