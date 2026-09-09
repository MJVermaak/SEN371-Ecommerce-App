using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;

namespace GrandmastersHub.Application.Helpers;

public static class InputSanitizer
{
    public static string Clean(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        input = input.Trim();

        input = Regex.Replace(input, @"\s+", " ");

        return input;
    }
}
