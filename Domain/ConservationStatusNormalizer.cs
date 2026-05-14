namespace BirdTaxonomy.API.Domain;
public static class ConservationStatusNormalizer
{
    private static readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        // Códigos directos
        ["LC"] = "LC",
        ["NT"] = "NT",
        ["VU"] = "VU",
        ["EN"] = "EN",
        ["CR"] = "CR",
        ["EX"] = "EX",
        ["EW"] = "EW",
        ["DD"] = "DD",
        ["NE"] = "NE",
        ["STABLE"] = "STABLE",
        // Alias en español
        ["estable"] = "STABLE",
        ["preocupacion menor"] = "LC",
        ["preocupación menor"] = "LC",
        ["casi amenazada"] = "NT",
        ["vulnerable"] = "VU",
        ["en peligro"] = "EN",
        ["en peligro critico"] = "CR",
        ["en peligro crítico"] = "CR",
        ["extinta"] = "EX",
        ["extinta en estado silvestre"] = "EW",
        ["datos insuficientes"] = "DD",
        ["no evaluada"] = "NE",
        // Alias en inglés
        ["least concern"] = "LC",
        ["near threatened"] = "NT",
        ["endangered"] = "EN",
        ["critically endangered"] = "CR",
        ["extinct"] = "EX",
        ["extinct in the wild"] = "EW",
        ["data deficient"] = "DD",
        ["not evaluated"] = "NE",
        ["stable"] = "STABLE",
    };

    public static string? Normalize(string? raw) =>
        string.IsNullOrWhiteSpace(raw) ? null :
        _map.TryGetValue(raw.Trim(), out var value) ? value : null;
}