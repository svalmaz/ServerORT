using System.Net.Mail;
using System.Net;

namespace ServerORT.Hub
{
    public class ResetPass
    {
        string pass = "tpnz wczm exjj tbod";
        public async Task SendEmail(string toAddress1, string subject1, string body1)
        {
            var fromAddress = new MailAddress("ort.pass.recovery@gmail.com", "ORT");
            var toAddress = new MailAddress(toAddress1, "Name");
            const string fromPassword = "tpnz wczm exjj tbod";
           

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject1,
                Body = body1
            })
            {
                smtp.Send(message);
            }
        }
    }
}
