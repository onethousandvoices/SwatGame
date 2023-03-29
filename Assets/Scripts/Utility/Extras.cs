using System.Globalization;
using System.Linq;

namespace SWAT.Utility
{
    public static class Extras
    {
        public const string Player = "operative";
        public const string PlayerWeapon = "weapon.operative";
        public const string Enemy = "enemy1";
        public const string EnemyWeapon_Pistol = "enemy.pistol";

        public static string BreakCamelCase(string str)
        {
            TextInfo text = new CultureInfo("en-US", false).TextInfo;
            string replaced = string.Concat(string.Concat(str.Split('_')).Select(c => char.IsUpper(c) || char.IsDigit(c)
                ? " " + c
                : c.ToString()));
            return text.ToTitleCase(replaced);
        }
    }
}