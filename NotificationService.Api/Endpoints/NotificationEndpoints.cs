using Microsoft.AspNetCore.Mvc;
using NotificationService.Api.DTOs;
using NotificationService.Core.Contracts;
using NotificationService.Core.Entities;
using NotificationService.Core.Enums;
using NotificationService.Infrastructure.Data;
using Rebus.Bus;

namespace NotificationService.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/notifications").WithTags("Notifications");

        group.MapPost("/", SubmitNotificationAsync);
    }

    private static async Task<IResult> SubmitNotificationAsync(
        [FromBody] CreateNotificationRequest request,
        [FromServices] NotificationDbContext db,
        [FromServices] IBus bus)
    {
        var record = new NotificationRecord
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = request.IdempotencyKey,
            UserId = request.UserId,
            Channel = request.Channel,
            Payload = request.Payload,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Notifications.Add(record);
        await db.SaveChangesAsync();

        await bus.Send(new SendNotificationCommand
        {
            NotificationId = record.Id,
            UserId = record.UserId,
            Channel = record.Channel,
            Payload = record.Payload
        });

        record.Status = NotificationStatus.Queued;
        await db.SaveChangesAsync();

        return Results.Accepted($"/v1/notifications/{record.Id}", new { id = record.Id, status = record.Status.ToString() });
    }
}
