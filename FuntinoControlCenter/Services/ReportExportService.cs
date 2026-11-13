using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace FuntinoControlCenter.Services
{
    internal static class ReportExportService
    {
        private const string SpreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        public static void ExportExcel(DataTable data, string title, string filePath)
        {
            if (data == null) throw new ArgumentNullException("data");
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                WriteText(archive, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/><Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/></Types>");
                WriteText(archive, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
                WriteText(archive, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"" + SpreadsheetNs + "\" xmlns:r=\"" + RelNs + "\"><sheets><sheet name=\"" + XmlSafe(title, 31) + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                WriteText(archive, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/><Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/></Relationships>");
                WriteText(archive, "xl/styles.xml", StylesXml());
                var entry = archive.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Optimal);
                using (var xml = XmlWriter.Create(entry.Open(), new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false }))
                {
                    xml.WriteStartDocument(true); xml.WriteStartElement("worksheet", SpreadsheetNs); xml.WriteAttributeString("xmlns", "r", null, RelNs);
                    xml.WriteStartElement("sheetViews"); xml.WriteStartElement("sheetView"); xml.WriteAttributeString("rightToLeft", "1"); xml.WriteAttributeString("workbookViewId", "0"); xml.WriteStartElement("pane"); xml.WriteAttributeString("ySplit", "1"); xml.WriteAttributeString("topLeftCell", "A2"); xml.WriteAttributeString("activePane", "bottomLeft"); xml.WriteAttributeString("state", "frozen"); xml.WriteEndElement(); xml.WriteEndElement(); xml.WriteEndElement();
                    xml.WriteStartElement("cols");
                    for (int i = 0; i < data.Columns.Count; i++) { xml.WriteStartElement("col"); xml.WriteAttributeString("min", (i + 1).ToString(CultureInfo.InvariantCulture)); xml.WriteAttributeString("max", (i + 1).ToString(CultureInfo.InvariantCulture)); xml.WriteAttributeString("width", Math.Max(12, Math.Min(34, data.Columns[i].ColumnName.Length + 4)).ToString(CultureInfo.InvariantCulture)); xml.WriteAttributeString("customWidth", "1"); xml.WriteEndElement(); }
                    xml.WriteEndElement(); xml.WriteStartElement("sheetData");
                    xml.WriteStartElement("row"); xml.WriteAttributeString("r", "1");
                    for (int c = 0; c < data.Columns.Count; c++) WriteCell(xml, ColumnName(c + 1) + "1", data.Columns[c].ColumnName, true);
                    xml.WriteEndElement();
                    for (int r = 0; r < data.Rows.Count; r++)
                    {
                        xml.WriteStartElement("row"); xml.WriteAttributeString("r", (r + 2).ToString(CultureInfo.InvariantCulture));
                        for (int c = 0; c < data.Columns.Count; c++) WriteCell(xml, ColumnName(c + 1) + (r + 2).ToString(CultureInfo.InvariantCulture), data.Rows[r][c], false);
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement(); xml.WriteEndElement(); xml.WriteEndDocument();
                }
            }
        }

        private static string StylesXml()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><styleSheet xmlns=\"" + SpreadsheetNs + "\"><fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Tahoma\"/></font><font><b/><color rgb=\"FFFFFFFF\"/><sz val=\"11\"/><name val=\"Tahoma\"/></font></fonts><fills count=\"3\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill><fill><patternFill patternType=\"solid\"><fgColor rgb=\"FF17484A\"/><bgColor indexed=\"64\"/></patternFill></fill></fills><borders count=\"2\"><border><left/><right/><top/><bottom/><diagonal/></border><border><left style=\"thin\"><color rgb=\"FFE0E7E5\"/></left><right style=\"thin\"><color rgb=\"FFE0E7E5\"/></right><top style=\"thin\"><color rgb=\"FFE0E7E5\"/></top><bottom style=\"thin\"><color rgb=\"FFE0E7E5\"/></bottom><diagonal/></border></borders><cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs><cellXfs count=\"2\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\"/><xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" xfId=\"0\"/></cellXfs><cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles></styleSheet>";
        }

        private static void WriteCell(XmlWriter xml, string address, object value, bool header)
        {
            xml.WriteStartElement("c"); xml.WriteAttributeString("r", address); if (header) xml.WriteAttributeString("s", "1");
            if (value == null || value == DBNull.Value) { xml.WriteEndElement(); return; }
            if (!header && IsNumeric(value)) { xml.WriteElementString("v", Convert.ToString(value, CultureInfo.InvariantCulture)); }
            else { xml.WriteAttributeString("t", "inlineStr"); xml.WriteStartElement("is"); xml.WriteStartElement("t"); xml.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve"); xml.WriteString(FormatValue(value)); xml.WriteEndElement(); xml.WriteEndElement(); }
            xml.WriteEndElement();
        }

        public static void ExportPdf(DataTable data, string title, string filePath)
        {
            if (data == null) throw new ArgumentNullException("data");
            var pages = new System.Collections.Generic.List<byte[]>();
            int pageSize = 22, pageCount = Math.Max(1, (int)Math.Ceiling(data.Rows.Count / (double)pageSize));
            for (int page = 0; page < pageCount; page++)
            {
                using (var bitmap = new Bitmap(1800, 1250, PixelFormat.Format24bppRgb))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias; graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    graphics.Clear(Color.FromArgb(247, 249, 247));
                    using (var brush = new SolidBrush(Color.FromArgb(20, 55, 58))) graphics.FillRectangle(brush, 0, 0, 1800, 160);
                    using (var titleFont = new Font("Tahoma", 28, FontStyle.Bold)) DrawRtl(graphics, title, titleFont, Brushes.White, new RectangleF(40, 35, 1710, 55));
                    using (var subtitleFont = new Font("Tahoma", 15))
                    using (var subtitleBrush = new SolidBrush(Color.FromArgb(206, 221, 216))) DrawRtl(graphics, "گزارش داده‌ها  |  " + DateTime.Now.ToString("yyyy/MM/dd  HH:mm"), subtitleFont, subtitleBrush, new RectangleF(40, 101, 1710, 32));
                    int visibleColumns = Math.Min(data.Columns.Count, 12); int right = 1745, tableTop = 200, colWidth = Math.Max(95, 1690 / Math.Max(1, visibleColumns));
                    using (var head = new SolidBrush(Color.FromArgb(224, 234, 230))) graphics.FillRectangle(head, 55, tableTop, 1690, 48);
                    for (int c = 0; c < visibleColumns; c++)
                    {
                        int x = right - (c + 1) * colWidth;
                        DrawCell(graphics, data.Columns[c].ColumnName, x, tableTop, colWidth, 48, true);
                    }
                    int start = page * pageSize, end = Math.Min(data.Rows.Count, start + pageSize); int rowHeight = 39;
                    for (int r = start; r < end; r++)
                    {
                        int y = tableTop + 48 + (r - start) * rowHeight;
                        if ((r - start) % 2 == 0) using (var light = new SolidBrush(Color.White)) graphics.FillRectangle(light, 55, y, 1690, rowHeight);
                        for (int c = 0; c < visibleColumns; c++) DrawCell(graphics, FormatValue(data.Rows[r][c]), right - (c + 1) * colWidth, y, colWidth, rowHeight, false);
                    }
                    using (var footerFont = new Font("Tahoma", 14))
                    using (var footerBrush = new SolidBrush(Color.FromArgb(65, 87, 84))) DrawRtl(graphics, "تعداد ردیف‌ها: " + data.Rows.Count.ToString("N0") + "     |     صفحه " + (page + 1) + " از " + pageCount, footerFont, footerBrush, new RectangleF(55, 1160, 1690, 35));
                    using (var memory = new MemoryStream())
                    {
                        var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.MimeType == "image/jpeg");
                        using (var parameters = new EncoderParameters(1)) { parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 88L); bitmap.Save(memory, encoder, parameters); }
                        pages.Add(memory.ToArray());
                    }
                }
            }
            WriteImagePdf(pages, filePath);
        }

        private static void DrawCell(Graphics g, string text, int x, int y, int w, int h, bool header)
        {
            using (var border = new Pen(Color.FromArgb(213, 223, 219))) g.DrawRectangle(border, x, y, w, h);
            using (var font = new Font("Tahoma", header ? 14 : 12, header ? FontStyle.Bold : FontStyle.Regular))
            using (var brush = new SolidBrush(header ? Color.FromArgb(20, 55, 58) : Color.FromArgb(40, 55, 53)))
                DrawRtl(g, text, font, brush, new RectangleF(x + 5, y + 3, w - 10, h - 6));
        }

        private static void DrawRtl(Graphics g, string text, Font font, Brush brush, RectangleF rect)
        {
            using (var format = new StringFormat(StringFormatFlags.DirectionRightToLeft)) { format.Alignment = StringAlignment.Near; format.LineAlignment = StringAlignment.Center; format.Trimming = StringTrimming.EllipsisCharacter; g.DrawString(text ?? "", font, brush, rect, format); }
        }

        private static void WriteImagePdf(System.Collections.Generic.List<byte[]> images, string path)
        {
            using (var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var offsets = new System.Collections.Generic.List<long> { 0 };
                byte[] header = Encoding.ASCII.GetBytes("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n"); output.Write(header, 0, header.Length);
                WriteObject(output, offsets, 1, Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"));
                var kids = String.Join(" ", Enumerable.Range(0, images.Count).Select(i => (3 + i * 3) + " 0 R"));
                WriteObject(output, offsets, 2, Encoding.ASCII.GetBytes("<< /Type /Pages /Kids [" + kids + "] /Count " + images.Count + " >>"));
                for (int i = 0; i < images.Count; i++)
                {
                    int pageId = 3 + i * 3, contentId = pageId + 1, imageId = pageId + 2;
                    WriteObject(output, offsets, pageId, Encoding.ASCII.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 864 600] /Resources << /XObject << /Im0 " + imageId + " 0 R >> >> /Contents " + contentId + " 0 R >>"));
                    byte[] content = Encoding.ASCII.GetBytes("q 864 0 0 600 0 0 cm /Im0 Do Q");
                    WriteStreamObject(output, offsets, contentId, "<< /Length " + content.Length + " >>", content);
                    byte[] img = images[i]; WriteStreamObject(output, offsets, imageId, "<< /Type /XObject /Subtype /Image /Width 1800 /Height 1250 /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " + img.Length + " >>", img);
                }
                long xref = output.Position; byte[] xrefHead = Encoding.ASCII.GetBytes("xref\n0 " + offsets.Count + "\n0000000000 65535 f \n"); output.Write(xrefHead, 0, xrefHead.Length);
                for (int i = 1; i < offsets.Count; i++) { byte[] line = Encoding.ASCII.GetBytes(offsets[i].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n"); output.Write(line, 0, line.Length); }
                byte[] trailer = Encoding.ASCII.GetBytes("trailer\n<< /Size " + offsets.Count + " /Root 1 0 R >>\nstartxref\n" + xref + "\n%%EOF"); output.Write(trailer, 0, trailer.Length);
            }
        }

        private static void WriteObject(Stream stream, System.Collections.Generic.List<long> offsets, int id, byte[] data)
        {
            offsets.Add(stream.Position); byte[] head = Encoding.ASCII.GetBytes(id + " 0 obj\n"); stream.Write(head, 0, head.Length); stream.Write(data, 0, data.Length); byte[] end = Encoding.ASCII.GetBytes("\nendobj\n"); stream.Write(end, 0, end.Length);
        }
        private static void WriteStreamObject(Stream stream, System.Collections.Generic.List<long> offsets, int id, string dictionary, byte[] data)
        {
            offsets.Add(stream.Position); byte[] head = Encoding.ASCII.GetBytes(id + " 0 obj\n" + dictionary + "\nstream\n"); stream.Write(head, 0, head.Length); stream.Write(data, 0, data.Length); byte[] end = Encoding.ASCII.GetBytes("\nendstream\nendobj\n"); stream.Write(end, 0, end.Length);
        }

        private static void WriteText(ZipArchive archive, string path, string text)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Optimal); using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false))) writer.Write(text);
        }
        private static string XmlSafe(string text, int max) { string s = (text ?? "گزارش").Replace("[", "(").Replace("]", ")").Replace("*", "-").Replace("?", ""); return s.Length > max ? s.Substring(0, max) : s; }
        private static string ColumnName(int n) { string name = ""; while (n > 0) { int rem = (n - 1) % 26; name = (char)('A' + rem) + name; n = (n - 1) / 26; } return name; }
        private static bool IsNumeric(object value) { return value is byte || value is short || value is int || value is long || value is decimal || value is double || value is float; }
        private static string FormatValue(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            if (value is DateTime) return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (value is byte[]) return "[binary]";
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}
