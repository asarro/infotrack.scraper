using System.Net;
using System.Text.RegularExpressions;
using Infotrack.Scraper.Configuration;

namespace Infotrack.Scraper.Scraping;

internal sealed class HtmlParsingEngine
{
    private static readonly Regex AnyOpenTagPattern = new(
        @"<(?<tag>[A-Za-z][A-Za-z0-9]*)\b(?<attrs>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ClassAttrPattern = new(
        @"\bclass=""(?<classes>[^""]*)""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex StripTagsPattern = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    public IReadOnlyList<ExtractedRecord> Parse(string html, ParsingRules rules)
    {
        var containers = ExtractBySelector(html, rules.ContainerSelector);

        return containers.Select(container =>
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in rules.Fields)
                fields[rule.Field] = ExtractField(container, rule);
            return new ExtractedRecord(fields);
        }).ToList();
    }

    private static string ExtractField(string html, FieldRule rule)
    {
        var element = ExtractBySelector(html, rule.Selector, rule.AttributeFilter).FirstOrDefault();
        if (element is null) return string.Empty;

        if (rule.ChildSelector is not null)
        {
            element = ExtractBySelector(element, rule.ChildSelector).FirstOrDefault();
            if (element is null) return string.Empty;
        }

        if (rule.Attribute is not null)
            return ExtractAttribute(element, rule.Attribute);

        if (rule.StopAt is not null)
            element = TruncateAt(element, rule.StopAt);

        return Decode(StripTagsPattern.Replace(element, string.Empty));
    }

    private static List<string> ExtractBySelector(string html, string selector, AttributeFilter? filter = null)
    {
        return selector.StartsWith('.')
            ? ExtractByClass(html, selector.TrimStart('.'), filter)
            : ExtractByTag(html, selector, filter);
    }

    private static List<string> ExtractByClass(string html, string className, AttributeFilter? filter)
    {
        var results = new List<string>();

        foreach (Match openMatch in AnyOpenTagPattern.Matches(html))
        {
            var attrs = openMatch.Groups["attrs"].Value;
            var classMatch = ClassAttrPattern.Match(attrs);
            if (!classMatch.Success) continue;

            var classes = classMatch.Groups["classes"].Value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!classes.Contains(className, StringComparer.OrdinalIgnoreCase)) continue;
            if (filter is not null && !HasAttribute(attrs, filter)) continue;

            var extracted = ExtractElement(html, openMatch.Groups["tag"].Value, openMatch.Index,
                openMatch.Index + openMatch.Length);
            if (extracted is not null)
                results.Add(extracted);
        }

        return results;
    }

    private static List<string> ExtractByTag(string html, string tagName, AttributeFilter? filter)
    {
        var results = new List<string>();

        foreach (Match openMatch in OpenTagPattern(tagName).Matches(html))
        {
            var attrs = openMatch.Groups["attrs"].Value;
            if (filter is not null && !HasAttribute(attrs, filter)) continue;

            var extracted = ExtractElement(html, tagName, openMatch.Index, openMatch.Index + openMatch.Length);
            if (extracted is not null)
                results.Add(extracted);
        }

        return results;
    }

    private static string? ExtractElement(string html, string tag, int openStart, int openEnd)
    {
        var depth = 1;
        var pos = openEnd;

        var sameTagOpen = OpenTagPattern(tag);
        var sameTagClose = CloseTagPattern(tag);

        while (depth > 0 && pos < html.Length)
        {
            var nextOpen = sameTagOpen.Match(html, pos);
            var nextClose = sameTagClose.Match(html, pos);

            if (!nextClose.Success) break;

            if (nextOpen.Success && nextOpen.Index < nextClose.Index)
            {
                depth++;
                pos = nextOpen.Index + nextOpen.Length;
            }
            else
            {
                depth--;
                if (depth == 0)
                    return html[openStart..(nextClose.Index + nextClose.Length)];
                pos = nextClose.Index + nextClose.Length;
            }
        }

        return null;
    }

    private static bool HasAttribute(string attrs, AttributeFilter filter)
    {
        var match = AttributeValuePattern(filter.Name).Match(attrs);
        return match.Success &&
               string.Equals(match.Groups["value"].Value, filter.Value, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractAttribute(string elementHtml, string attributeName)
    {
        var openTag = AnyOpenTagPattern.Match(elementHtml);
        if (!openTag.Success) return string.Empty;

        var match = AttributeValuePattern(attributeName).Match(openTag.Groups["attrs"].Value);
        return match.Success ? Decode(match.Groups["value"].Value) : string.Empty;
    }

    private static string TruncateAt(string elementHtml, string stopAtSelector)
    {
        var openTag = AnyOpenTagPattern.Match(elementHtml);
        if (!openTag.Success) return elementHtml;

        var innerStart = openTag.Index + openTag.Length;
        var innerHtml = elementHtml[innerStart..];
        var stopPos = FindFirstSelectorPosition(innerHtml, stopAtSelector);

        return stopPos < 0
            ? elementHtml
            : elementHtml[..(innerStart + stopPos)];
    }

    private static int FindFirstSelectorPosition(string html, string selector)
    {
        if (selector.StartsWith('.'))
        {
            var className = selector.TrimStart('.');
            foreach (Match m in AnyOpenTagPattern.Matches(html))
            {
                var classMatch = ClassAttrPattern.Match(m.Groups["attrs"].Value);
                if (!classMatch.Success) continue;
                var classes = classMatch.Groups["classes"].Value
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (classes.Contains(className, StringComparer.OrdinalIgnoreCase))
                    return m.Index;
            }
        }
        else
        {
            var match = OpenTagPattern(selector).Match(html);
            if (match.Success) return match.Index;
        }

        return -1;
    }

    private static Regex OpenTagPattern(string tagName) =>
        new($@"<{Regex.Escape(tagName)}\b(?<attrs>[^>]*)>", RegexOptions.IgnoreCase);

    private static Regex CloseTagPattern(string tagName) =>
        new($@"</{Regex.Escape(tagName)}\s*>", RegexOptions.IgnoreCase);

    private static Regex AttributeValuePattern(string attributeName) =>
        new($@"\b{Regex.Escape(attributeName)}=""(?<value>[^""]*)""", RegexOptions.IgnoreCase);

    private static string Decode(string text) =>
        WebUtility.HtmlDecode(text.Trim()).Replace('\u00a0', ' ').Trim();
}