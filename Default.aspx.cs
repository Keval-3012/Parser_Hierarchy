using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;

public partial class _Default : Page
{
    private const string SampleText =
@"Hierarchy:
1. International Financial Services Centres Authority Act, 2019 2. Complaint Handling and Grievance Redressal by Regulated Entities in the IFSC at IFSC Circular F. No. IFSCA-LPRA/3/2024, dt. 02.12.24/ Central/ Corporate Secretarial

Hierarchy:
1. Information Technology Act, 2000 2. Information Technology (Procedure and Safeguards for Interception, Monitoring and Decryption of Information) Rules, 2009/ Central/ Data Privacy/Information Technology

1. Gujarat Shops and Establishments Act, 2019 2. Gujarat Shops Rules, 2020/ Gujarat/ Labour Law

1. Karnataka Industrial Employment (Standing Orders) Rules, 1961/ Karnataka/ Labour Law

1. Some Act, 2015 2. Some Rules, 2016 (no scope or category here)";

    // Optional master: Category -> SubCategory. Load from DB in a real project.
    private static Dictionary<string, string> GetSubCategoryMap()
    {
        Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // map.Add("Labour Law", "Shops and Establishments");   // example override
        return map;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void btnSample_Click(object sender, EventArgs e)
    {
        txtInput.Text = SampleText;
        BindResults();
    }

    protected void btnParse_Click(object sender, EventArgs e)
    {
        BindResults();
    }

    protected void btnExport_Click(object sender, EventArgs e)
    {
        DataTable dt = BuildTable();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("RowNo,Level1,Level2,OtherLevels,Scope,Category,SubCategory,Status");
        foreach (DataRow r in dt.Rows)
        {
            sb.AppendLine(string.Join(",", new string[]
            {
                Csv(r["RowNo"]), Csv(r["Level1"]), Csv(r["Level2"]), Csv(r["OtherLevels"]),
                Csv(r["Scope"]), Csv(r["Category"]), Csv(r["SubCategory"]),
                Csv((bool)r["IsValid"] ? "OK" : "CHECK: " + r["Error"])
            }));
        }

        Response.Clear();
        Response.ContentType = "text/csv";
        Response.AddHeader("Content-Disposition", "attachment; filename=HierarchyResult.csv");
        Response.Write(sb.ToString());
        Response.End();
    }

    private void BindResults()
    {
        DataTable dt = BuildTable();
        gvResult.DataSource = dt;
        gvResult.DataBind();

        int ok = 0;
        foreach (DataRow r in dt.Rows)
            if ((bool)r["IsValid"]) ok++;

        lblSummary.Text = dt.Rows.Count == 0
            ? ""
            : string.Format("<br />Parsed {0} entries: {1} OK, {2} need review.", dt.Rows.Count, ok, dt.Rows.Count - ok);
    }

    private DataTable BuildTable()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("RowNo", typeof(int));
        dt.Columns.Add("Level1", typeof(string));
        dt.Columns.Add("Level2", typeof(string));
        dt.Columns.Add("OtherLevels", typeof(string));
        dt.Columns.Add("Scope", typeof(string));
        dt.Columns.Add("Category", typeof(string));
        dt.Columns.Add("SubCategory", typeof(string));
        dt.Columns.Add("IsValid", typeof(bool));
        dt.Columns.Add("Error", typeof(string));

        Dictionary<string, string> subMap = GetSubCategoryMap();
        int n = 0;

        foreach (string entry in SplitEntries(txtInput.Text))
        {
            n++;
            HierarchyResult r = HierarchyParser.Parse(entry, subMap);

            DataRow row = dt.NewRow();
            row["RowNo"] = n;
            row["Level1"] = r.Levels.Count > 0 ? r.Levels[0] : "";
            row["Level2"] = r.Levels.Count > 1 ? r.Levels[1] : "";
            row["OtherLevels"] = r.Levels.Count > 2 ? string.Join(" | ", r.Levels.GetRange(2, r.Levels.Count - 2).ToArray()) : "";
            row["Scope"] = r.Scope ?? "";
            row["Category"] = r.Category ?? "";
            row["SubCategory"] = r.SubCategory ?? "";
            row["IsValid"] = r.IsValid;
            row["Error"] = r.Error ?? "";
            dt.Rows.Add(row);
        }
        return dt;
    }

    // An entry starts at a line beginning with "Hierarchy:" or after a blank line.
    private static List<string> SplitEntries(string text)
    {
        List<string> entries = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return entries;

        string[] lines = text.Replace("\r", "").Split('\n');
        StringBuilder current = new StringBuilder();

        foreach (string line in lines)
        {
            bool blank = line.Trim().Length == 0;
            bool newHeader = line.TrimStart().StartsWith("Hierarchy:", StringComparison.OrdinalIgnoreCase);

            if ((blank || newHeader) && current.Length > 0)
            {
                // a header line right after the "Hierarchy:" label alone belongs to the same entry
                if (!(newHeader && current.ToString().Trim().Length == 0))
                {
                    entries.Add(current.ToString().Trim());
                    current.Length = 0;
                }
            }
            if (!blank) current.Append(line).Append(' ');
        }
        if (current.ToString().Trim().Length > 0)
            entries.Add(current.ToString().Trim());

        return entries;
    }

    private static string Csv(object value)
    {
        string s = value == null ? "" : value.ToString();
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            s = "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}
