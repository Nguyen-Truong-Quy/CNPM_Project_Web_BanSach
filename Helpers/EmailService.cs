using System;
using System.Net;
using System.Net.Mail;

namespace CNPM_Project_web.Helpers
{
    public class EmailService
    {
        public static void SendOtp(string toEmail, string otp)
        {
            string fromEmail = "quy1chatgpt@gmail.com";
            string fromPassword = "ctcb wbes csrj ybtv"; // App Password

            try
            {
                var fromAddress = new MailAddress(fromEmail, "Nhà sách HandMake");
                var toAddress = new MailAddress(toEmail);
                const string subject = "Mã xác thực OTP";
                string body = $"<h3>Mã OTP của bạn là: <strong>{otp}</strong></h3>";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message); // Gửi email ở đây
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
                throw;
            }
        }
    }
}