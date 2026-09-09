using System;
using System.Collections.Generic;
using System.Text;

namespace GrandmastersHub.Application.Helpers;

public static class ValidationHelper
{
    public static bool IsValidPrice(decimal price)
    {
        return price > 0;
    }

    public static bool IsValidStock(int stock)
    {
        return stock >= 0;
    }

    public static bool IsNotEmpty(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}
