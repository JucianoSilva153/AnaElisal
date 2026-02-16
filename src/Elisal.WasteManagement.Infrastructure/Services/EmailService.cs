using System;
using System.Threading.Tasks;
using Elisal.WasteManagement.Application.Common;
using Elisal.WasteManagement.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Elisal.WasteManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, 
                _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            
            if (!string.IsNullOrEmpty(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log error or rethrow
            Console.WriteLine($"Error sending email: {ex.Message}");
            throw;
        }
    }

    public async Task SendCredentialsEmailAsync(string to, string name, string password)
    {
        string subject = "Suas Credenciais de Acesso - Elisal SGR";
        string body = $@"
            <h2>Olá {name},</h2>
            <p>Sua conta no Sistema de Gestão de Resíduos (SGR) da Elisal foi criada com sucesso.</p>
            <p>Utilize as credenciais abaixo para seu primeiro acesso:</p>
            <ul>
                <li><strong>Utilizador:</strong> {to}</li>
                <li><strong>Senha Temporária:</strong> {password}</li>
            </ul>
            <p>Por favor, altere sua senha no primeiro acesso através do seu perfil.</p>
            <br/>
            <p>Atenciosamente,<br/>Equipa IT Elisal</p>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendResetPasswordEmailAsync(string to, string name, string token)
    {
        string subject = "Recuperação de Senha - Elisal SGR";
        string body = $@"
            <h2>Olá {name},</h2>
            <p>Recebemos uma solicitação para redefinir sua senha.</p>
            <p>Sua nova senha temporária é: <strong>{token}</strong></p>
            <p>Se você não solicitou isso, por favor entre em contato com o suporte.</p>
            <br/>
            <p>Atenciosamente,<br/>Equipa IT Elisal</p>";

        await SendEmailAsync(to, subject, body);
    }
}
