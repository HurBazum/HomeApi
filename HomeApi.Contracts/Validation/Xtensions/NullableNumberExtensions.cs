namespace HomeApi.Contracts
{
    /// <summary>
    /// Класс-расширение для int?
    /// нужен для валидации 
    /// </summary>
    public static class NullableNumberExtensions
    {
        public static bool GreaterThanZeroOrIsNull(this int? x)
        {
            if((x != null && x > 0) || x == null) 
                return true;
            else
                return false;
        }
    }
}