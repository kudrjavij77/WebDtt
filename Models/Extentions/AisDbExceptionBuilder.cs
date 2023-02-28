using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebDtt.Models.Extentions
{
    public class AisDbExceptionBuilder
    {
        private StringBuilder outerMessage = new StringBuilder();
        private string _original;
        public string HumanMessage => outerMessage.ToString();
        public AisDbExceptionBuilder(string message, int errorCode, StringBuilder sb)
        {
            _original = message;
            if (errorCode == 547)
                FKExceptionBuilder();
            if (errorCode == 2601)
                UKExceptionBuilder();
            var msg1 = sb.ToString();
            if (msg1 != message && outerMessage.Length == 0)
                outerMessage.Append(msg1);
            if (outerMessage.Length == 0)
                outerMessage.Append(message);
        }

        private void UKExceptionBuilder()
        {
            outerMessage.Append("Попытка вставки дублирующейся записи в таблицу ");
            var splitted = _original.Split('\r', '\n').First().Split(' ');
            //var tableName = splitted[Array.IndexOf(splitted, "object") + 1].Split('.').Last().Replace("\'", "").Replace(",", "");
            //var tableDesc = ObjectExtensions.GetDescription(tableName);
            //outerMessage.Append($"\"{tableDesc}\":\r\n");
            outerMessage.Append("Запись ");
            var start = Array.IndexOf(splitted, "is") + 1;
            for (var i = start; i < splitted.Length; i++)
                outerMessage.AppendFormat("{0} ", splitted[i]);
            outerMessage.AppendLine("уже существует.\r\nОперация прервана.");
        }
        private void FKExceptionBuilder()
        {
            var splitted = _original.Split('\r', '\n').First().Split(' ');
            outerMessage.Append("Ошибка ");
            switch (splitted[1])
            {
                case "UPDATE":
                    outerMessage.Append("обновления ");
                    break;
                case "INSERT":
                    outerMessage.Append("создания ");
                    break;
                case "DELETE":
                    outerMessage.Append("удаления ");
                    break;
            }

            //outerMessage.AppendLine("объекта.");
            //if (Array.IndexOf(splitted, "REFERENCE") > -1)
            //{
            //    var tableName = splitted[Array.IndexOf(splitted, "table") + 1].Split('.').Last().Replace("\"", "").Replace(",", "");
            //    var propertyName = splitted[Array.IndexOf(splitted, "column") + 1].Replace("'", "").Replace(".", "");
            //    var tableDesc = ObjectExtensions.GetDescription(tableName);
            //    var propertyDesc = ObjectExtensions.GetDescription(propertyName);
            //    outerMessage.AppendLine($"На объект ссылается таблица [{tableDesc}], поле [{propertyDesc}].");
            //    outerMessage.AppendLine($"Ошибка индекса {splitted[Array.IndexOf(splitted, "constraint") + 1]}");
            //}

            outerMessage.AppendLine("Операция прервана.");
        }
    }
}