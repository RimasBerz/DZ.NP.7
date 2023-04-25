using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Http
{
    public partial class SmtpWindow : Window
    {
        private dynamic? emailConfig;

        public SmtpWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            String configFilename = "emailconfig.json";
            try
            {
                emailConfig = JsonSerializer.Deserialize<dynamic>(
                    System.IO.File.ReadAllText(configFilename)
                );
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show($"Не найден файл конфигурации '{configFilename}'");
                this.Close();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Ошибка преобразования конфигурации '{ex.Message}'");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки конфигурации '{ex.Message}'");
                this.Close();
            }
            if (emailConfig is null)
            {
                MessageBox.Show("Ошибка получения конфигурации");
                this.Close();
            }
        }

        private SmtpClient GetSmtpClient()
        {
            if (emailConfig is null) { return null!; }
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");

            String host = gmail.GetProperty("host").GetString()!;
            int port = gmail.GetProperty("port").GetInt32();
            String mailbox = gmail.GetProperty("email").GetString()!;
            String password = gmail.GetProperty("password").GetString()!;
            bool ssl = gmail.GetProperty("ssl").GetBoolean();

            return new(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
        }



        private void SendTest2ButtonButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(mailbox),
                Body = "<u>Test</u> <i>message</i> from <b style='color:green'>SmtpWindow</b>",
                IsBodyHtml = true,
                Subject = "Test Message"
            };
            mailMessage.To.Add(new MailAddress("7crimas@gmail.com"));

            try
            {
                smtpClient.Send(mailMessage);
                MessageBox.Show("Sent OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }
        }

        private void SendTestButtonButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            try
            {
                smtpClient.Send(
                    from: mailbox,
                    recipients: "7crimas@gmail.com",
                    subject: "Test Message",
                    body: "Test message from SmtpWindow");

                MessageBox.Show("Sent OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }
        }
        static Random random = new Random();

        private void SendPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            const string chars = "abcdefghijkmnopqrstuvwxy23456789";
            string password = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            JsonElement smtpConfig = emailConfig.GetProperty("smtp");
            JsonElement gmailConfig = smtpConfig.GetProperty("gmail");

            SmtpClient smtpClient = new SmtpClient()
            {
                Host = gmailConfig.GetProperty("host").GetString(),
                Port = gmailConfig.GetProperty("port").GetInt32(),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    gmailConfig.GetProperty("email").GetString(),
                    gmailConfig.GetProperty("password").GetString())
            };
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(gmailConfig.GetProperty("email").GetString()),
                Body = $"Ваш новый пароль: {password}",
                IsBodyHtml = false,
                Subject = "Новый пароль для входа"
            };
            mailMessage.To.Add(new MailAddress("7crimas@gmail.com"));

            try
            {
                smtpClient.Send(mailMessage);
                MessageBox.Show("Пароль отправлен на указанный адрес электронной почты");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки сообщения: {ex.Message}");
            }
        }
    }
}