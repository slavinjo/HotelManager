using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Hotel.Server.Data;
using Hotel.Server.Helpers;
using Hotel.Server.Integrations;

namespace Hotel.Server.Notifications;

public class NotificationService
{
    private readonly IMailgunService _mailgunService;

    public NotificationService(IMailgunService mailgunService)
    {
        _mailgunService = mailgunService;
    }

    public async Task SendEmail(string to, string subject, string text, string from = null,
        string emailFromOverride = null)
    {
        await _mailgunService.SendEmail(to, subject, text, from, emailFromOverride);
    }
}
