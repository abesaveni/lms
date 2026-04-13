using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

namespace LiveExpert.API.Services;

/// <summary>
/// Generates a proper vector PDF from the AI-produced markdown resume text.
/// Replaces the old html2canvas + jsPDF approach that produced 5–10 MB image-based PDFs.
/// Output is typically 50–200 KB with real selectable text that ATS scanners can parse.
/// </summary>
public class ResumePdfService
{
    private record TextPart(string Text, bool Bold);

    static ResumePdfService()
    {
        // QuestPDF Community licence — free for revenue under $1M/year
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateFromMarkdown(string markdownText)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor("#2d2d2d"));

                page.Content().Column(col =>
                {
                    col.Spacing(0);
                    RenderMarkdown(col, markdownText);
                });
            });
        }).GeneratePdf();
    }

    // ── Markdown renderer ─────────────────────────────────────────────────────

    private static void RenderMarkdown(ColumnDescriptor col, string text)
    {
        var lines = text.Split('\n');
        bool headerDone = false;
        bool firstLine = true;
        var bulletBuffer = new List<string>();

        void FlushBullets()
        {
            if (bulletBuffer.Count == 0) return;
            col.Item().PaddingLeft(8).PaddingTop(1).Column(bulletCol =>
            {
                bulletCol.Spacing(2);
                foreach (var b in bulletBuffer)
                {
                    bulletCol.Item().Row(row =>
                    {
                        row.ConstantItem(12).Text("•").FontSize(9).FontColor("#4f46e5");
                        row.RelativeItem().Text(txt =>
                        {
                            foreach (var part in ParseInline(b))
                            {
                                var span = txt.Span(part.Text).FontSize(9.5f);
                                if (part.Bold) span.Bold();
                            }
                        });
                    });
                }
            });
            bulletBuffer.Clear();
        }

        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.Trim();

            // Blank line — flush any buffered bullets and skip
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                FlushBullets();
                continue;
            }

            // Bullet line (starts with * or -)
            var bulletMatch = Regex.Match(trimmed, @"^[*\-]\s+(.+)");
            if (bulletMatch.Success)
            {
                bulletBuffer.Add(bulletMatch.Groups[1].Value);
                continue;
            }

            FlushBullets();

            // ── First non-empty line = candidate name ────────────────────────
            if (firstLine)
            {
                firstLine = false;
                var name = trimmed.Replace("**", "").Trim();
                col.Item().AlignCenter().Text(name).FontSize(22).Bold().FontColor("#1a1a2e");
                col.Item().Height(4);
                continue;
            }

            // ── Contact line (before first section header) ───────────────────
            if (!headerDone &&
                (trimmed.Contains('|') ||
                 Regex.IsMatch(trimmed, @"^(Email|Phone|Location|LinkedIn|GitHub|Website):", RegexOptions.IgnoreCase)))
            {
                col.Item().AlignCenter().Text(txt =>
                {
                    foreach (var part in ParseInline(trimmed))
                    {
                        var span = txt.Span(part.Text).FontSize(8.5f).FontColor("#555555");
                        if (part.Bold) span.Bold();
                    }
                });
                col.Item().Height(1);
                continue;
            }

            // ── ALL-CAPS section header ──────────────────────────────────────
            if (IsSectionHeader(trimmed))
            {
                headerDone = true;
                var label = trimmed.Replace("**", "").Trim();
                col.Item()
                    .PaddingTop(10)
                    .PaddingBottom(3)
                    .BorderBottom(1.5f)
                    .BorderColor("#c7d2fe")
                    .Text(label)
                    .FontSize(10f)
                    .Bold()
                    .FontColor("#4f46e5");
                col.Item().Height(2);
                continue;
            }

            // ── Role / sub-header line inside a section ──────────────────────
            // Detected by: inside body, contains |, starts with bold or uppercase
            if (headerDone &&
                trimmed.Contains('|') &&
                (trimmed.Contains("**") || (trimmed.Length > 0 && char.IsUpper(trimmed[0]))))
            {
                col.Item().PaddingTop(6).Text(txt =>
                {
                    foreach (var part in ParseInline(trimmed))
                    {
                        var span = txt.Span(part.Text).FontSize(9.5f).FontColor("#1a1a2e");
                        if (part.Bold) span.Bold();
                    }
                });
                continue;
            }

            // ── Default: normal paragraph ────────────────────────────────────
            col.Item().Text(txt =>
            {
                foreach (var part in ParseInline(trimmed))
                {
                    var span = txt.Span(part.Text).FontSize(9.5f);
                    if (part.Bold) span.Bold();
                }
            });
        }

        FlushBullets();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns true when the line (ignoring ** wrappers) is an ALL-CAPS section header.</summary>
    private static bool IsSectionHeader(string raw)
    {
        var text = raw.Replace("**", "").Trim();
        return text.Length >= 4 &&
               Regex.IsMatch(text, @"^[A-Z][A-Z\s&/]+$") &&
               text == text.ToUpperInvariant();
    }

    /// <summary>Splits a line on **bold** markers and returns typed text parts.</summary>
    private static List<TextPart> ParseInline(string text)
    {
        var result = new List<TextPart>();
        var remaining = text;

        while (remaining.Length > 0)
        {
            var m = Regex.Match(remaining, @"\*\*([^*]+)\*\*");
            if (!m.Success)
            {
                // No more bold markers — append rest (strip any stray **)
                result.Add(new TextPart(remaining.Replace("**", ""), false));
                break;
            }

            if (m.Index > 0)
                result.Add(new TextPart(remaining[..m.Index], false));

            result.Add(new TextPart(m.Groups[1].Value, true));
            remaining = remaining[(m.Index + m.Length)..];
        }

        return result.Count > 0 ? result : [new TextPart(text, false)];
    }
}
