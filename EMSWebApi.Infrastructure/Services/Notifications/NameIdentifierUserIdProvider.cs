using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EMSWebApi.Infrastructure.Services.Notifications
{
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public virtual string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
