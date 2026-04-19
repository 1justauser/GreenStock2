using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenStock.Services;

/// <summary>
/// Перечисление поддерживаемых валют.
/// </summary>
public enum Currency
{
    RUB = 0,
    USD = 1,
    EUR = 2
}

/// <summary>
/// Сервис для работы с валютами и преобразованием цен.
/// </summary>
public static class CurrencyService
{
    private static Dictionary<string, decimal> _rates = new()
    {
        { "USD", 1.0m },      // Базовая валюта
        { "EUR", 1.1m },
        { "RUB", 98.0m }
    };

    private static Currency _currentCurrency = Currency.RUB;

    /// <summary>
    /// Получить текущую выбранную валюту.
    /// </summary>
    public static Currency CurrentCurrency => _currentCurrency;

    /// <summary>
    /// Установить текущую валюту.
    /// </summary>
    public static void SetCurrency(Currency currency)
    {
        _currentCurrency = currency;
    }

    /// <summary>
    /// Получить курс валюты к базовой (USD).
    /// </summary>
    public static decimal GetRate(Currency currency)
    {
        return currency switch
        {
            Currency.RUB => _rates["RUB"],
            Currency.USD => _rates["USD"],
            Currency.EUR => _rates["EUR"],
            _ => 1.0m
        };
    }

    /// <summary>
    /// Установить курсы валют (для обновления из API).
    /// </summary>
    public static void SetRates(Dictionary<string, decimal> rates)
    {
        if (rates != null)
        {
            foreach (var kvp in rates)
            {
                if (_rates.ContainsKey(kvp.Key))
                    _rates[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Конвертировать сумму из одной валюты в другую.
    /// </summary>
    public static decimal Convert(decimal amount, Currency from, Currency to)
    {
        if (from == to) return amount;

        // Сначала конвертируем в USD (базовую валюту)
        var inUSD = amount / GetRate(from);

        // Затем конвертируем из USD в целевую валюту
        return inUSD * GetRate(to);
    }

    /// <summary>
    /// Получить символ валюты.
    /// </summary>
    public static string GetSymbol(Currency currency)
    {
        return currency switch
        {
            Currency.RUB => "₽",
            Currency.USD => "$",
            Currency.EUR => "€",
            _ => ""
        };
    }

    /// <summary>
    /// Получить код валюты.
    /// </summary>
    public static string GetCode(Currency currency)
    {
        return currency switch
        {
            Currency.RUB => "RUB",
            Currency.USD => "USD",
            Currency.EUR => "EUR",
            _ => ""
        };
    }

    /// <summary>
    /// Форматировать сумму с символом валюты.
    /// </summary>
    public static string Format(decimal amount, Currency? currency = null)
    {
        currency ??= _currentCurrency;
        var symbol = GetSymbol(currency.Value);
        return $"{amount:N2} {symbol}";
    }

    /// <summary>
    /// Получить все доступные валюты.
    /// </summary>
    public static List<Currency> GetAvailableCurrencies()
    {
        return new List<Currency> { Currency.RUB, Currency.USD, Currency.EUR };
    }
}
