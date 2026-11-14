using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Activity1_FinalTerm
{
    internal static class LocalSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Activity1_FinalTerm",
            "settings.txt");

        public static string Currency { get; private set; } = "PHP";
        public static decimal TaxRate { get; private set; } = 10m;

        static LocalSettings()
        {
            Load();
        }

        public static void Save(string currency, decimal taxRate)
        {
            Currency = currency.Trim().ToUpperInvariant();
            TaxRate = taxRate;
            string directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllLines(SettingsPath, new[]
            {
                "Currency=" + Currency,
                "TaxRate=" + TaxRate.ToString(CultureInfo.InvariantCulture)
            });
        }

        private static void Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string line in File.ReadAllLines(SettingsPath))
                {
                    int separator = line.IndexOf('=');
                    if (separator > 0) values[line.Substring(0, separator)] = line.Substring(separator + 1);
                }

                string currency;
                if (values.TryGetValue("Currency", out currency) && !string.IsNullOrWhiteSpace(currency))
                    Currency = currency.Trim().ToUpperInvariant();

                string taxRate;
                decimal parsedRate;
                if (values.TryGetValue("TaxRate", out taxRate)
                    && decimal.TryParse(taxRate, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedRate)
                    && parsedRate >= 0 && parsedRate <= 100)
                {
                    TaxRate = parsedRate;
                }
            }
            catch
            {
                Currency = "PHP";
                TaxRate = 10m;
            }
        }
    }
}
