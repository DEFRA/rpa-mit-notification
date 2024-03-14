using System;
using System.Text;

namespace RPA.MIT.Notification.Function.Services;

public static class MessageDecoder
{
    public static string DecodeMessage(this string message)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(message));
    }
}
