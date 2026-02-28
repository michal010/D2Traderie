using System.Collections.Generic;
using System.Linq;

namespace D2Traderie.Project.Consts
{
    /// <summary>
    /// Wartości walut przeliczone na jednostki całkowite (user_value * 10000).
    /// Ber Rune = 35000, El Rune = 8, Perfect Amethyst = 27 itd.
    /// Źródło: https://traderie.com/api/diablo2resurrected/items/values
    /// </summary>
    public static class RunesValue
    {
        // Lista wyświetlana w UI (kolejność od najcenniejszych)
        public static List<string> Runes => RuneValues.Keys.ToList();

        public static Dictionary<string, ulong> RuneValues = new Dictionary<string, ulong>()
        {
            // === RUNY ===
            { "Ber Rune",   35000 },
            { "Jah Rune",   30000 },
            { "Sur Rune",   17500 },
            { "Lo Rune",    12500 },
            { "Zod Rune",    9000 },
            { "Ohm Rune",    7500 },
            { "Vex Rune",    5000 },
            { "Cham Rune",   4000 },
            { "Gul Rune",    2500 },
            { "Ist Rune",    1600 },
            { "Mal Rune",    1000 },
            { "Um Rune",      600 },
            { "Pul Rune",     400 },
            { "Lem Rune",     200 },
            { "Fal Rune",     100 },
            { "Ko Rune",      100 },
            { "Lum Rune",     100 },
            { "Io Rune",       16 },
            { "Hel Rune",      16 },
            { "Dol Rune",       8 },
            { "Shael Rune",     8 },
            { "Sol Rune",       8 },
            { "Amn Rune",       8 },
            { "Thul Rune",      8 },
            { "Ort Rune",       8 },
            { "Ral Rune",       8 },
            { "Tal Rune",       8 },
            { "Ith Rune",       8 },
            { "Eth Rune",       8 },
            { "Nef Rune",       8 },
            { "Tir Rune",       8 },
            { "Eld Rune",       8 },
            { "El Rune",        8 },

            // === GEMY ===
            { "Perfect Amethyst",  27 },
            { "Perfect Topaz",      8 },
            { "Perfect Ruby",       8 },
            { "Perfect Diamond",    8 },
            { "Perfect Emerald",    8 },
            { "Perfect Sapphire",   8 },
            { "Perfect Skull",      8 },
        };
    }
}