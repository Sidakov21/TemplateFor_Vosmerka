using System.Linq;
using System;

namespace TemplateFor_Vosmerka
{
    internal class Core
    {
        public static DB_DemoExzEntities Context = new DB_DemoExzEntities();
        public static User LoggedUser;
    }

    public partial class Product
    {
        // Название продукта по макету
        public string Name => ProductName;

        // Список материалов через запятую
        public string MaterialsList
        {
            get
            {
                if (Make == null || Make.Count == 0)
                    return "Нет материалов";

                var list = Make
                    .Select(m => m.Material?.MaterialName)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                return list.Count > 0 ? string.Join(", ", list) : "Нет материалов";
            }
        }

        public string ImagePath
        {
            get
            {
                if (!string.IsNullOrEmpty(Image))
                {
                    string image = Image.Trim().ToLower();
                    if (image != "не указано" && image != "нет" && image != "отсутствует")
                    {
                        return "/" + Image.TrimStart('\\', '/').Replace('\\', '/');
                    }
                }

                return "/Images/picture.png";
            }
        }

        // Превышает ли минимальная стоимость 10000 руб.
        public bool IsExpensive => MinCost > 10000;
    }

}
