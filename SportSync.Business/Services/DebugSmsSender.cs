using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SportSync.Business.Interfaces;

namespace SportSync.Business.Services
{
    public class DebugSmsSender : ISmsSender
    {
        private readonly ILogger<DebugSmsSender> _logger;

        public DebugSmsSender(ILogger<DebugSmsSender> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simulates sending an SMS by logging the message content.
        /// </summary>
        /// <param name="phoneNumber">The recipient's phone number.</param>
        /// <param name="message">The message content.</param>
        /// <returns>A completed task.</returns>
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            string logMessage = $"[DebugSmsSender] To: {phoneNumber} | Message: \"{message}\"";

            // Log to ILogger (visible in console output, e.g., ASP.NET Core console)
            _logger.LogInformation("Simulating SMS send: {LogMessage}", logMessage);

            // Log to Debug output (visible in Visual Studio's Debug Output window)
            Debug.WriteLine(logMessage);

            // In a real scenario, you would integrate with an actual SMS gateway API here.
            // For debugging OTPs, the message (which contains the OTP) is now logged.

            return Task.CompletedTask;
        }
    }   
}
