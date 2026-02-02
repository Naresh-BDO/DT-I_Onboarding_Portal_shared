
using DT_I_Onboarding_Portal.Core.Models;
using DT_I_Onboarding_Portal.Data;
using DT_I_Onboarding_Portal.Core.enums;
using DT_I_Onboarding_Portal.Core.Models.Dto;
using DT_I_Onboarding_Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DT_I_Onboarding_Portal.Server.Controllers
{
    [ApiController]
    [Route("api/new-joiners")]
    public class NewJoinersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _email;

        public NewJoinersController(ApplicationDbContext db, IEmailSender email)
        {
            _db = db;
            _email = email;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateNewJoiner([FromBody] CreateNewJoinerDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (dto.Email is null)
                return BadRequest(new { message = "Email is required." });

            // Normalize email
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            // Guard against duplicates (same email + start date)
            var exists = await _db.NewJoiners
                .AnyAsync(nj => nj.Email.ToLower() == normalizedEmail && nj.StartDate.Date == dto.StartDate.Date);
            if (exists)
            {
                return Conflict(new { message = "A new joiner with this email and start date already exists." });
            }

            var newJoiner = new NewJoiner
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                Department = dto.Department?.Trim(),
                ManagerName = dto.ManagerName?.Trim(),
                StartDate = dto.StartDate.Date, // store date component (no time)
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.NewJoiners.Add(newJoiner);
            await _db.SaveChangesAsync(); // Save first so we have an Id even if email fails

            // Compose welcome email (put your actual HTML content here)
            var subject = $"Welcome to the team, {newJoiner.FullName}!";
            var html = $@"
        <div style=""font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#333"">
            <h2>Welcome, {newJoiner.FullName} 👋</h2>
            <p>We’re excited to have you join the <strong>{newJoiner.Department ?? "team"}</strong> on <strong>{newJoiner.StartDate:MMMM dd, yyyy}</strong>.</p>
            {(string.IsNullOrWhiteSpace(newJoiner.ManagerName) ? "" : $"<p>Your manager will be <strong>{newJoiner.ManagerName}</strong>.</p>")}
            <p>Before your first day, please check your email for onboarding tasks and credentials.</p>
            <hr />
            <p>If you have any questions, reply to this email.</p>
            <p>— Onboarding Team</p>
        </div>";

            // Send email with detailed result
            var sendResult = await _email.SendEmailAsync(newJoiner.Email, subject, html);

            if (sendResult.Success)
            {
                newJoiner.WelcomeEmailSentAtUtc = DateTime.UtcNow;
                newJoiner.LastSendError = null;
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = newJoiner.Id }, new
                {
                    id = newJoiner.Id,
                    fullName = newJoiner.FullName,
                    email = newJoiner.Email,
                    startDate = newJoiner.StartDate,
                    welcomeEmailSentAtUtc = newJoiner.WelcomeEmailSentAtUtc
                });
            }

            // Failure path: persist raw error and return 202 Accepted with diagnostics
            newJoiner.LastSendError = $"{sendResult.ErrorType}: {sendResult.ProviderMessage ?? sendResult.ErrorMessage}";
            await _db.SaveChangesAsync();

            var advice = sendResult.ErrorType switch
            {
                EmailErrorType.AuthenticationFailed => "Check SMTP Username/Password (App Password if using Gmail/Office 365 with MFA) and allow SMTP AUTH.",
                EmailErrorType.SmtpConnectionFailed => "Verify SMTP Host/Port/TLS and that outbound port 587 is open.",
                EmailErrorType.Timeout => "SMTP timed out. Try again or check network connectivity/firewall.",
                EmailErrorType.RecipientRejected => "Recipient address may not exist or is blocked. Verify the email address.",
                EmailErrorType.InvalidRecipientAddress => "The recipient email format is invalid.",
                EmailErrorType.ConfigurationError => "SMTP settings (Host/Port/FromAddress) are incomplete.",
                EmailErrorType.SmtpSendFailed => "SMTP send failed. Check SPF/DKIM/DMARC and mail server policies.",
                _ => "Unknown error. Check logs and SMTP server status."
            };

            return Accepted(new
            {
                id = newJoiner.Id,
                message = "New joiner created, but failed to send welcome email.",
                errorType = sendResult.ErrorType.ToString(),
                error = sendResult.ErrorMessage,
                providerMessage = sendResult.ProviderMessage,
                advice
            });
        }


        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var nj = await _db.NewJoiners.FindAsync(id);
            if (nj == null) return NotFound();

            return Ok(new
            {
                nj.Id,
                nj.FullName,
                nj.Email,
                nj.Department,
                nj.ManagerName,
                nj.StartDate,
                nj.CreatedAtUtc,
                nj.WelcomeEmailSentAtUtc,
                nj.LastSendError
            });
        }
    }
}
