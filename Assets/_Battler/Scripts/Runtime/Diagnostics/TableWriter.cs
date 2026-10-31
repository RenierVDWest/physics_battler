using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// A CSV table written a row at a time, gzip-compressed. The first row's column names become the
    /// header, so every row must add the same columns in the same order (blank when not applicable).
    /// Numbers are written with a fixed number of decimals, without allocating.
    /// </summary>
    internal sealed class TableWriter : IDisposable
    {
        private static readonly long[] Pow10 = { 1, 10, 100, 1000, 10000, 100000 };

        private readonly Stream file;
        private readonly GZipStream gzip;
        private readonly StreamWriter writer;
        private readonly StringBuilder line = new StringBuilder(4096);
        private readonly List<string> header = new List<string>();
        private readonly char[] digits = new char[24];
        private bool headerWritten;
        private bool disposed;
        private int column;

        public int Rows { get; private set; }

        public TableWriter(string path)
        {
            file = File.Create(path);
            gzip = new GZipStream(file, CompressionLevel.Fastest);
            writer = new StreamWriter(gzip, new UTF8Encoding(false), 1 << 16);
        }

        public void Begin()
        {
            line.Clear();
            column = 0;
        }

        private void Name(string name)
        {
            if (!headerWritten)
            {
                header.Add(name);
            }

            if (column++ > 0)
            {
                line.Append(',');
            }
        }

        public void Add(string name, float value, int decimals)
        {
            Name(name);
            AppendFixed(value, decimals);
        }

        public void Add(string name, int value)
        {
            Name(name);
            AppendLong(value);
        }

        public void Add(string name, bool value)
        {
            Name(name);
            line.Append(value ? '1' : '0');
        }

        /// <summary>A short word (no commas or quotes).</summary>
        public void Add(string name, string value)
        {
            Name(name);
            if (value != null)
            {
                line.Append(value);
            }
        }

        public void Blank(string name)
        {
            Name(name);
        }

        public void End()
        {
            if (!headerWritten)
            {
                writer.WriteLine(string.Join(",", header));
                headerWritten = true;
            }

            writer.WriteLine(line);
            Rows++;
        }

        public void Flush()
        {
            if (disposed || Rows == 0)
            {
                return;   // nothing written yet: some runtimes' gzip streams fail flushing an empty stream
            }

            writer.Flush();
            gzip.Flush();
            file.Flush();
        }

        /// <summary>
        /// Closes the table. The file itself is always closed, even if finishing the compressed stream
        /// fails: a file left open stays locked by Unity, and Git can't commit it (first playtest).
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            try
            {
                writer.Dispose();   // finishes the gzip stream and closes the file
            }
            catch (Exception)
            {
                // Fall through: close what is left below.
            }
            finally
            {
                try { gzip.Dispose(); } catch (Exception) { /* already closed or failed */ }
                try { file.Dispose(); } catch (Exception) { /* already closed */ }
            }
        }

        private void AppendFixed(float value, int decimals)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return;   // blank
            }

            decimals = Math.Max(0, Math.Min(decimals, Pow10.Length - 1));
            long scale = Pow10[decimals];
            long n = (long)Math.Round((double)value * scale);
            if (n < 0)
            {
                line.Append('-');
                n = -n;
            }

            AppendLong(n / scale);
            if (decimals > 0)
            {
                line.Append('.');
                long fraction = n % scale;
                for (int d = decimals - 1; d >= 0; d--)
                {
                    line.Append((char)('0' + (int)(fraction / Pow10[d] % 10)));
                }
            }
        }

        private void AppendLong(long value)
        {
            if (value < 0)
            {
                line.Append('-');
                value = -value;
            }

            int count = 0;
            do
            {
                digits[count++] = (char)('0' + (int)(value % 10));
                value /= 10;
            }
            while (value > 0 && count < digits.Length);

            for (int i = count - 1; i >= 0; i--)
            {
                line.Append(digits[i]);
            }
        }
    }
}
