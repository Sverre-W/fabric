using Azure.Identity;
using Fabric.Server.Core;
using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Notifications;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Notifications.Services;

public sealed class EmailNotificationSender(
    IOptions<EmailOptions> options,
    ITenantContext tenantContext,
    ILogger<EmailNotificationSender> logger)
{
    private const string GraphEndpoint = "https://graph.microsoft.com/.default";

    public async Task<Result<EmailErrors>> SendEmail(string subject, string body, object model, IEnumerable<string> receivers, IEnumerable<string>? carbonCopy = null, CancellationToken ct = default)
    {
        string[] cc = carbonCopy?.ToArray() ?? [];
        string[] to = receivers.ToArray();

        if (to.Length == 0)
            return Result.Failure(EmailErrors.InvalidEmail);

        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure(EmailErrors.EmptySubject);

        GraphEmailSettings? emailSettings = tenantContext.Configuration.GraphEmail ?? options.Value.Graph;
        if (emailSettings is null || !emailSettings.IsConfigured())
        {
            logger.EmailNotConfigured();
            return Result.Failure(EmailErrors.NotConfigured);
        }

        try
        {
            var credential = new ClientSecretCredential(emailSettings.AzureTenantId, emailSettings.ApplicationId, emailSettings.Secret);

            var graphClient = new GraphServiceClient(credential, [GraphEndpoint]);

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody { ContentType = BodyType.Html, Content = body },
                Sender = new Recipient
                {
                    EmailAddress = new EmailAddress { Address = emailSettings.FromEmail, Name = emailSettings.FromName },
                },
                ToRecipients = ToRecipients(to),
            };

            if (cc.Length != 0)
            {
                message.CcRecipients = ToRecipients(cc);
            }

            await graphClient
                .Users[emailSettings.FromEmail]
                .SendMail.PostAsync(
                    new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                    {
                        Message = message,
                        SaveToSentItems = emailSettings.SaveSentItems,
                    },
                    cancellationToken: ct
                );

            return Result.Success<EmailErrors>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.EmailSendFailed(ex, string.Join(", ", to));
            return Result.Failure(EmailErrors.GraphFailed);
        }
    }

    private static List<Recipient> ToRecipients(IEnumerable<string> addresses)
    {
        return addresses.Select(addr => new Recipient { EmailAddress = new EmailAddress { Address = addr } }).ToList();
    }
}


public enum EmailErrors
{
    NotConfigured,
    InvalidEmail,
    EmptySubject,
    GraphFailed
}

internal static partial class EmailNotificationSenderLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Email notification skipped because Graph email is not configured.")]
    public static partial void EmailNotConfigured(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send email to {Receivers} via Graph API.")]
    public static partial void EmailSendFailed(this ILogger logger, Exception exception, string receivers);
}
