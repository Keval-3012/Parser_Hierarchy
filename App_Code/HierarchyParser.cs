using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Fully dynamic parser. No fixed Scope list.
///
/// Input pattern:   [legal text] / [Scope] / [Category]
///   e.g. "1. IFSC Act, 2019 2. Circular F. No. IFSCA-LPRA/3/2024, dt. 02.12.24/ Central/ Corporate Secretarial"
///
/// Rule: the SECTION separator is a slash followed by whitespace ("/ "), with NO space before the slash.
/// These slashes are part of the text and are NOT split:
///   - no space after  : IFSCA-LPRA/3/2024, Data Privacy/Information Technology
///   - space before it : Internal - Agreement / Deed   (" / ")
///
/// From the end:   last section = Category,  second-last = Scope,  everything before = legal levels.
/// Works with C# 4+/.NET Framework 4.x (classic ASP.NET WebForms).
/// </summary>
public class HierarchyResult
{
    public string Raw { get; set; }
    public List<string> Levels { get; set; }   // 1. Act, 2. Rules / Circular, ...
    public string Scope { get; set; }          // whatever appears in the scope position
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public bool IsValid { get; set; }          // false => send to manual review
    public string Error { get; set; }

    public HierarchyResult()
    {
        Levels = new List<string>();
    }
}

public static class HierarchyParser
{
    // Section separator: "/" followed by at least one whitespace char (unchanged rule),
    // AND (new condition) the slash must be attached to the previous word, i.e. NOT preceded by a space.
    //   "Concession Agreement/ Central/ Internal - Agreement / Deed"
    //        separator ->    ^^          ^^        (slash right after a word, space after)
    //        NOT a separator:                                  ^^^  " / " has a space BEFORE the slash,
    //                                                               so "Agreement / Deed" stays in one category.
    private static readonly Regex SectionSplit = new Regex(@"(?<=\S)/\s+", RegexOptions.Compiled);

    // Numbering of legal levels: "1. ", "2. " (must be at start or after whitespace).
    private static readonly Regex LevelSplit = new Regex(@"(?<!\S)\d+\.\s+", RegexOptions.Compiled);

    /// <param name="input">Full hierarchy text from the row.</param>
    /// <param name="subCategoryMap">Optional Category -> SubCategory master (case-insensitive dictionary
    /// recommended). If null or category not found, SubCategory = Category.</param>
    public static HierarchyResult Parse(string input, IDictionary<string, string> subCategoryMap = null)
    {
        HierarchyResult result = new HierarchyResult();
        result.Raw = input;

        if (string.IsNullOrWhiteSpace(input))
        {
            result.Error = "Empty input";
            return result;
        }

        // 1. Normalize: drop "Hierarchy:" prefix, collapse spaces / new lines
        string text = Regex.Replace(input, @"^\s*Hierarchy\s*:\s*", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\s+", " ").Trim();

        // 2. Split into sections on "/ "
        string[] sections = SectionSplit.Split(text);

        if (sections.Length < 3)
        {
            result.Error = "Could not find '/ Scope/ Category' at the end";
            return result;
        }

        // 3. From the end: Category, then Scope; everything before is legal text
        string category = sections[sections.Length - 1].Trim();
        string scope = sections[sections.Length - 2].Trim();

        string legalText = string.Join("/ ", sections, 0, sections.Length - 2).Trim();

        if (scope.Length == 0 || category.Length == 0)
        {
            result.Error = "Scope or Category is empty";
            return result;
        }

        result.Scope = scope;
        result.Category = category;

        // 4. Sub Category: from master if present, else same as Category
        string sub;
        if (subCategoryMap != null && subCategoryMap.TryGetValue(category, out sub))
            result.SubCategory = sub;
        else
            result.SubCategory = category;

        // 5. Legal levels: split on "1. ", "2. " numbering
        foreach (string p in LevelSplit.Split(legalText))
        {
            string level = p.Trim().TrimEnd(',', ';');
            if (level.Length > 0)
                result.Levels.Add(level);
        }

        result.IsValid = result.Levels.Count > 0;
        if (!result.IsValid) result.Error = "No legal levels found";
        return result;
    }
}

/* ---------------- USAGE in aspx.cs ----------------

// Optional master for sub categories (can be loaded from DB). Pass null if not needed.
Dictionary<string, string> subMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

HierarchyResult r = HierarchyParser.Parse(txtHierarchy.Text, subMap);

if (r.IsValid)
{
    lblScope.Text    = r.Scope;         // any value: Central, Gujarat, Maharashtra, Karnataka, ...
    lblCategory.Text = r.Category;
    lblSub.Text      = r.SubCategory;
    lblLevel1.Text   = r.Levels[0];
}
else
{
    lblError.Text = r.Error;            // flag for manual review
}
-------------------------------------------------- */
