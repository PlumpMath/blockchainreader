using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace blockchain_parser.Model
{
    public class EmailNotificationsHelper : DataBaseHelper
    {
        public EmailNotifications GetEmailNotification(string template_name, int language) {
            return Read(db => db.EmailNotifications, condition => (condition.SlugName == template_name && condition.Language == language.ToString()));
        }
    }
}