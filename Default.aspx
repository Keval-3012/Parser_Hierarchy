<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Hierarchy Parser Demo</title>
    <style type="text/css">
        body { font-family: Segoe UI, Arial, sans-serif; margin: 24px; color: #222; }
        h2 { margin-bottom: 4px; }
        .hint { color: #666; margin-bottom: 14px; }
        .box { width: 100%; max-width: 1100px; height: 170px; font-family: Consolas, monospace; font-size: 13px; }
        .btn { padding: 8px 18px; margin-right: 8px; cursor: pointer; }
        .grid { border-collapse: collapse; margin-top: 18px; width: 100%; max-width: 1400px; }
        .grid th { background: #1f4e79; color: #fff; text-align: left; padding: 8px; }
        .grid td { border: 1px solid #ccc; padding: 6px 8px; vertical-align: top; font-size: 13px; }
        .ok { color: #1b7f3b; font-weight: 600; }
        .bad { color: #b00020; font-weight: 600; }
        .summary { margin-top: 14px; font-weight: 600; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Hierarchy Parser Demo</h2>
        <div class="hint">
            Paste one hierarchy per line. Format: <code>1. Act 2. Rules / Circular/ Scope/ Category</code>
            (sections are separated by a slash followed by a space).
            Several lines of one entry can be joined with a blank line between entries.
        </div>

        <asp:TextBox ID="txtInput" runat="server" TextMode="MultiLine" CssClass="box" />
        <br /><br />
        <asp:Button ID="btnParse" runat="server" Text="Parse" CssClass="btn" OnClick="btnParse_Click" />
        <asp:Button ID="btnSample" runat="server" Text="Load sample data" CssClass="btn" OnClick="btnSample_Click" />
        <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn" OnClick="btnExport_Click" />

        <asp:Label ID="lblSummary" runat="server" CssClass="summary" />

        <asp:GridView ID="gvResult" runat="server" AutoGenerateColumns="false" CssClass="grid"
            GridLines="None" EmptyDataText="No rows parsed yet.">
            <Columns>
                <asp:BoundField DataField="RowNo" HeaderText="#" />
                <asp:BoundField DataField="Level1" HeaderText="Level 1" />
                <asp:BoundField DataField="Level2" HeaderText="Level 2" />
                <asp:BoundField DataField="OtherLevels" HeaderText="Other Levels" />
                <asp:BoundField DataField="Scope" HeaderText="Scope" />
                <asp:BoundField DataField="Category" HeaderText="Category" />
                <asp:BoundField DataField="SubCategory" HeaderText="Sub Category" />
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <span class='<%# (bool)Eval("IsValid") ? "ok" : "bad" %>'>
                            <%# (bool)Eval("IsValid") ? "OK" : "CHECK: " + Eval("Error") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>
