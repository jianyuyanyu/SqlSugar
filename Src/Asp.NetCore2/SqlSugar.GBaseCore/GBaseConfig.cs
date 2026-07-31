namespace SqlSugar.GBase
{
    public class GBaseConfig
    {
        public static string SqlTranslationLeft()
        {
            return GBaseConfig.IsMySqlMode
                ? "`" : "";
        }

        public static string SqlTranslationRight()
        {
            return GBaseConfig.IsMySqlMode
                ? "`" : "";
        }

        public static bool IsMySqlMode { get; set; }
    }
}
