using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Interfaces
{
    public interface ISmsSender
    {
        /// <summary>
        /// Sends an SMS message asynchronously.
        /// </summary>
        /// <param name="phoneNumber">The recipient's phone number.</param>
        /// <param name="message">The message content to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendSmsAsync(string phoneNumber, string message);
    }
}
