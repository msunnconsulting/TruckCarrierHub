namespace Common.Utility.Email
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Mail;
    using System.Net.Mime;

    /// <summary>
    /// This is a SMTPClient class.
    /// </summary>
    public static class SMTPClient
    {
        /// <summary>
        /// This is a method for sending mail which sends email by reading Email settings from web.config
        /// </summary>
        /// <param name="toEmails">Enter id to which you want to send an Email</param>
        /// <param name="subject">Enter subject</param>
        /// <param name="body">Enter text message which you want to send</param>
        /// <param name="replaceValues">Enter replace value from dictionary. by default it is null</param>
        /// <param name="attachments">Enter attachments</param>
        /// <param name="fromName">Enter name of sender</param>
        /// <param name="fromEmail">Enter Email id of sender</param>
        /// <param name="isHTML">indicates that your message contains html or not</param>
        /// <param name="ccEmails">Enter cc</param>
        /// <param name="bccEmails">Enter bcc</param>
        /// <param name="replyToEmails">by default it is null</param>
        /// <param name="priority">Enter Priority</param>        
        /// <returns>indicates that mail is sent or not</returns>
        public static void Send(string toEmails, string subject, string body, string fromEmail, Dictionary<string, string> replaceValues = null,
            string[] attachments = null, string fromName = null, bool isHTML = true, string ccEmails = null, string bccEmails = null,
            string replyToEmails = null, MailPriority priority = MailPriority.Normal)
        {
            MailMessage mailMessage = new MailMessage();
            try
            {
                if (string.IsNullOrEmpty(toEmails))
                    throw new ArgumentNullException("toEmails");

                if (string.IsNullOrEmpty(subject))
                    throw new ArgumentNullException("subject");

                if (string.IsNullOrEmpty(body))
                    throw new ArgumentNullException("body");

                if (string.IsNullOrEmpty(fromEmail))
                    throw new ArgumentNullException("fromEmail");

                if (string.IsNullOrEmpty(fromName))
                    fromName = fromEmail;   //// mailMessage.From.Address

                MailAddress fromAddress;
                fromAddress = new MailAddress(fromEmail, fromName);

                mailMessage.From = fromAddress;

                mailMessage.To.Add(toEmails);

                if (!string.IsNullOrEmpty(ccEmails))
                    mailMessage.CC.Add(ccEmails);

                if (!string.IsNullOrEmpty(bccEmails))
                    mailMessage.Bcc.Add(bccEmails);

                if (!string.IsNullOrEmpty(replyToEmails))
                    mailMessage.ReplyToList.Add(replyToEmails);

                mailMessage.Subject = subject;

                //// replace key in dictionary
                if (replaceValues != null)
                {
                    foreach (KeyValuePair<string, string> kvp in replaceValues)
                    {
                        body = body.Replace(kvp.Key, kvp.Value);
                    }
                }

                mailMessage.Body = body;
                mailMessage.IsBodyHtml = isHTML;

                if (attachments != null)
                {
                    ////add the attachments
                    if (attachments != null && attachments.Length > 0)
                    {
                        foreach (var file in attachments)
                        {
                            if (!System.IO.File.Exists(file)) continue;

                            var data = new Attachment(file, MediaTypeNames.Application.Octet);
                            //// Add time stamp information for the file.
                            var disposition = data.ContentDisposition;
                            disposition.CreationDate = System.IO.File.GetCreationTime(file);
                            disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                            disposition.ReadDate = System.IO.File.GetLastAccessTime(file);

                            mailMessage.Attachments.Add(data);
                        }
                    }
                }

                mailMessage.Priority = priority;

                SmtpClient smtpCl = new SmtpClient();
                // smtpCl.EnableSsl = enableSSL; -- this is taken from web.config

                smtpCl.Send(mailMessage);

            }
            finally
            {
                foreach (System.Net.Mail.Attachment attachment in mailMessage.Attachments)
                {
                    attachment.Dispose();
                }
            }
        }


        /// <summary>
        /// This is a method for sending mail which sends email based on login, SSL, Port etc. information you supply to this method 
        /// </summary>
        /// <param name="toEmails">Enter id to which you want to send an Email</param>
        /// <param name="subject">Enter subject</param>
        /// <param name="body">Enter text message which you want to send</param>
        /// <param name="fromEmail">Enter Email id of sender</param>
        /// <param name="smtpHost">Enter host for SMTL configuration</param>
        /// <param name="smtpPort">Enter SMTP configuration port setting for email sending</param>
        /// <param name="smtpEnableSSL">Enter SSL setting for security</param>
        /// <param name="smtpUserName">Enter SMTP email sending credentials username</param>
        /// <param name="smtpPassword">Enter SMTP email sending credentials password</param>
        /// <param name="replaceValues">Enter replace value from dictionary. by default it is null</param>
        /// <param name="attachments">Enter attachments</param>
        /// <param name="fromName">Enter name of sender</param>
        /// <param name="isHTML">indicates that your message contains html or not</param>
        /// <param name="ccEmails">Enter cc</param>
        /// <param name="bccEmails">Enter bcc</param>
        /// <param name="replyToEmails">by default it is null</param>
        /// <param name="priority">Enter Priority</param>
        /// <returns>indicates that mail is sent or not</returns>
        public static void Send(string toEmails, string subject, string body, string fromEmail, string smtpHost, int smtpPort, bool smtpEnableSSL,
            string smtpUserName, string smtpPassword, Dictionary<string, string> replaceValues = null, string[] attachments = null,
            string fromName = null, bool isHTML = true, string ccEmails = null, string bccEmails = null,
            string replyToEmails = null, MailPriority priority = MailPriority.Normal)
        {
            MailMessage mailMessage = new MailMessage();
            try
            {
                if (string.IsNullOrEmpty(toEmails))
                    throw new ArgumentNullException("toEmails");

                if (string.IsNullOrEmpty(subject))
                    throw new ArgumentNullException("subject");

                if (string.IsNullOrEmpty(body))
                    throw new ArgumentNullException("body");

                if (string.IsNullOrEmpty(fromEmail))
                    throw new ArgumentNullException("fromEmail");

                if (string.IsNullOrEmpty(smtpHost))
                    throw new ArgumentNullException("smtpHost");

                if (smtpPort == 0)
                    throw new ArgumentNullException("smtpPort");

                if (string.IsNullOrEmpty(smtpUserName))
                    throw new ArgumentNullException("smtpUserName");
                if (string.IsNullOrEmpty(smtpPassword))
                    throw new ArgumentNullException("smtpPassword");


                if (string.IsNullOrEmpty(fromName))
                    fromName = fromEmail;

                MailAddress fromAddress;
                fromAddress = new MailAddress(fromEmail, fromName);

                mailMessage.From = fromAddress;

                mailMessage.To.Add(toEmails);

                //if (!string.IsNullOrEmpty(toEmails))
                //    mailMessage.To.Add(toEmails);

                if (!string.IsNullOrEmpty(ccEmails))
                    mailMessage.CC.Add(ccEmails);

                if (!string.IsNullOrEmpty(bccEmails))
                    mailMessage.Bcc.Add(bccEmails);

                if (!string.IsNullOrEmpty(replyToEmails))
                    mailMessage.ReplyToList.Add(replyToEmails);

                mailMessage.Subject = subject;

                //// replace key in dictionary
                if (replaceValues != null)
                {
                    foreach (KeyValuePair<string, string> kvp in replaceValues)
                    {
                        body = body.Replace(kvp.Key, kvp.Value);
                    }
                }

                mailMessage.Body = body;
                mailMessage.IsBodyHtml = isHTML;
                if (attachments != null)
                {
                    ////add the attachments
                    if (attachments != null && attachments.Length > 0)
                    {
                        foreach (var file in attachments)
                        {
                            if (!System.IO.File.Exists(file)) continue;

                            var data = new Attachment(file, MediaTypeNames.Application.Octet);
                            //// Add time stamp information for the file.
                            var disposition = data.ContentDisposition;
                            disposition.CreationDate = System.IO.File.GetCreationTime(file);
                            disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
                            disposition.ReadDate = System.IO.File.GetLastAccessTime(file);

                            mailMessage.Attachments.Add(data);

                        }
                    }
                }

                mailMessage.Priority = priority;

                SmtpClient smtpCl = new SmtpClient
                {
                    Host = smtpHost,
                    Port = smtpPort,
                    EnableSsl = smtpEnableSSL,
                    Credentials = new NetworkCredential(smtpUserName, smtpPassword)
                };

                smtpCl.Send(mailMessage);
            }
            finally
            {
                foreach (System.Net.Mail.Attachment attachment in mailMessage.Attachments)
                {
                    attachment.Dispose();
                }
            }
        }
    }
}
