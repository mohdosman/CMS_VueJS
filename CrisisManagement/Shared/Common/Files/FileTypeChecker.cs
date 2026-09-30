// Ported from SafetyNet (Shared/Common/Files/FileTypeChecker.cs). Kept as it is there, so it compiles without nullable annotations.
#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CrisisManagement.Shared.Common.Files
{
    /// <summary>
    /// Content-aware upload gate: an allow list of file types the caller expects, plus a
    /// deny list of types nobody may upload under any name.
    ///
    /// Two rules, deliberately asymmetric:
    ///   * Permit needs the extension AND the content signature to agree, so a .exe renamed
    ///     to .pdf never satisfies the PDF entry.
    ///   * Deny fires on extension OR content, so a renamed executable is caught by its magic
    ///     bytes and a .bat - which has no magic bytes at all - is caught by its extension.
    ///
    /// An empty permit list allows nothing. Fail closed: every caller states its allow list.
    ///
    /// Permit and Deny each return a new instance rather than mutating this one, so a checker is
    /// safe to build once and hold for the life of the process - which is how it is registered.
    /// </summary>
    public sealed class FileTypeChecker
    {
        private readonly FileSignature[] _permit;
        private readonly FileSignature[] _deny;

        public FileTypeChecker()
            : this(Array.Empty<FileSignature>(), Array.Empty<FileSignature>())
        { }

        private FileTypeChecker(FileSignature[] permit, FileSignature[] deny)
        {
            _permit = permit;
            _deny = deny;
        }

        public FileTypeChecker Permit(params FileSignature[] signatures) =>
            new(_permit.Concat(signatures).ToArray(), _deny);

        public FileTypeChecker Deny(params FileSignature[] signatures) =>
            new(_permit, _deny.Concat(signatures).ToArray());

        /// <summary>
        /// True when the file is on the permit list and on no deny list. <paramref name="match"/>
        /// carries the identified type, whose MimeType is what should be recorded for the upload -
        /// never the browser-supplied content type.
        /// </summary>
        public bool TryIdentify(string fileName, ReadOnlySpan<byte> data, out FileSignature match)
        {
            match = null;

            if (Denies(fileName, data) != null)
                return false;

            string extension = ExtensionOf(fileName);
            if (extension == null)
                return false;

            foreach (FileSignature signature in _permit)
            {
                if (signature.MatchesExtension(extension) && signature.ContentAgrees(data))
                {
                    match = signature;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The deny entry this file trips, or null. Exposed so callers can log *why* an upload
        /// was rejected - somebody pushing an executable is worth a log line.
        /// </summary>
        public FileSignature Denies(string fileName, ReadOnlySpan<byte> data)
        {
            string extension = ExtensionOf(fileName);

            foreach (FileSignature signature in _deny)
            {
                if ((extension != null && signature.MatchesExtension(extension)) || signature.MatchesContent(data))
                    return signature;
            }

            return null;
        }

        /// <summary>Lower-case extension without the dot, or null when the name has no usable one.</summary>
        private static string ExtensionOf(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            // GetExtension returns "" for a name with no dot and for a trailing dot, which is
            // what we want - unlike Split('.').Last(), which hands back the whole filename.
            string extension = Path.GetExtension(fileName);

            return extension.Length > 1 ? extension.Substring(1).ToLowerInvariant() : null;
        }
    }

    /// <summary>
    /// One file type: the extensions it is allowed to wear and the magic numbers its bytes must
    /// carry. Within a position group the magic numbers are alternatives (any one hits); across
    /// groups every populated group must hit.
    /// </summary>
    public sealed class FileSignature
    {
        private enum Position { Start, End, Anywhere }

        private readonly string[] _extensions;
        private readonly Magic[] _prefixes;
        private readonly Magic[] _suffixes;
        private readonly Magic[] _anywhere;

        public string Name { get; }
        public string MimeType { get; }
        public IReadOnlyList<string> Extensions => _extensions;

        /// <summary>False for types that are pure text and have nothing to sniff for (txt, csv, bat).</summary>
        public bool HasMagic => _prefixes.Length + _suffixes.Length + _anywhere.Length > 0;

        public FileSignature(
            string name,
            string mimeType,
            string[] extensions,
            Magic[] prefixes = null,
            Magic[] suffixes = null,
            Magic[] anywhere = null)
        {
            Name = name;
            MimeType = mimeType;
            _extensions = (extensions ?? Array.Empty<string>()).Select(e => e.ToLowerInvariant()).ToArray();
            _prefixes = prefixes ?? Array.Empty<Magic>();
            _suffixes = suffixes ?? Array.Empty<Magic>();
            _anywhere = anywhere ?? Array.Empty<Magic>();
        }

        public bool MatchesExtension(string extension) =>
            _extensions.Contains(extension);

        /// <summary>True only when there is something to check and the bytes pass it.</summary>
        public bool MatchesContent(ReadOnlySpan<byte> data) =>
            HasMagic
            && MatchesAny(_prefixes, data, Position.Start)
            && MatchesAny(_suffixes, data, Position.End)
            && MatchesAny(_anywhere, data, Position.Anywhere);

        /// <summary>The permit-side test: nothing to sniff means the extension stands alone.</summary>
        public bool ContentAgrees(ReadOnlySpan<byte> data) =>
            !HasMagic || MatchesContent(data);

        private static bool MatchesAny(Magic[] magics, ReadOnlySpan<byte> data, Position position)
        {
            if (magics.Length == 0)
                return true; // nothing constrained at this position

            foreach (Magic magic in magics)
            {
                bool hit = position switch
                {
                    Position.Start => magic.IsPrefixOf(data),
                    Position.End => magic.IsSuffixOf(data),
                    _ => magic.IsContainedIn(data),
                };

                if (hit)
                    return true;
            }

            return false;
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// A magic number: the expected bytes, an optional equal-length mask AND-ed over the file's
    /// bytes before comparing (so "don't care" positions can be zeroed), and an offset - measured
    /// from the start for prefixes and back from the end for suffixes.
    /// </summary>
    public sealed class Magic
    {
        private readonly byte[] _bytes;
        private readonly byte[] _mask;
        private readonly int _offset;

        public Magic(byte[] bytes, byte[] mask = null, int offset = 0)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentException("A magic number needs at least one byte.", nameof(bytes));

            if (mask != null && mask.Length != bytes.Length)
                throw new ArgumentException("Mask must be the same length as the magic number.", nameof(mask));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            _bytes = bytes;
            _mask = mask;
            _offset = offset;
        }

        /// <summary>ASCII convenience overload - most signatures are printable.</summary>
        public Magic(string ascii, byte[] mask = null, int offset = 0)
            : this(Encoding.ASCII.GetBytes(ascii), mask, offset)
        { }

        public bool IsPrefixOf(ReadOnlySpan<byte> data) =>
            _offset + _bytes.Length <= data.Length
            && Equal(data.Slice(_offset, _bytes.Length));

        public bool IsSuffixOf(ReadOnlySpan<byte> data) =>
            _offset + _bytes.Length <= data.Length
            && Equal(data.Slice(data.Length - _offset - _bytes.Length, _bytes.Length));

        /// <summary>Anywhere in the file. The offset does not apply.</summary>
        public bool IsContainedIn(ReadOnlySpan<byte> data)
        {
            if (_mask == null)
                return data.IndexOf(_bytes.AsSpan()) >= 0;

            for (int i = 0; i + _bytes.Length <= data.Length; i++)
            {
                if (Equal(data.Slice(i, _bytes.Length)))
                    return true;
            }

            return false;
        }

        private bool Equal(ReadOnlySpan<byte> window)
        {
            if (_mask == null)
                return window.SequenceEqual(_bytes);

            for (int i = 0; i < _bytes.Length; i++)
            {
                if ((byte)(window[i] & _mask[i]) != _bytes[i])
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// The catalog. Signatures from https://www.garykessler.net/library/file_sigs.html.
    ///
    /// ponytail: one signature per type, prefixes only. Suffix and floating-number support exists
    /// on <see cref="Magic"/> but nothing here needs it - requiring a PDF's trailing %%EOF or a
    /// zip's end-of-central-directory record rejects real files that carry trailing bytes.
    /// </summary>
    public static class FileSignatures
    {
        private static byte[] Bytes(params byte[] bytes) => bytes;

        // --- permitted -------------------------------------------------------------------

        public static readonly FileSignature Pdf = new(
            "PDF", "application/pdf", new[] { "pdf" },
            prefixes: new[] { new Magic("%PDF-") });

        public static readonly FileSignature Jpeg = new(
            "JPEG", "image/jpeg", new[] { "jpg", "jpeg" },
            prefixes: new[] { new Magic(Bytes(0xFF, 0xD8, 0xFF)) });

        public static readonly FileSignature Png = new(
            "PNG", "image/png", new[] { "png" },
            prefixes: new[] { new Magic(Bytes(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)) });

        public static readonly FileSignature Gif = new(
            "GIF", "image/gif", new[] { "gif" },
            prefixes: new[] { new Magic("GIF87a"), new Magic("GIF89a") });

        public static readonly FileSignature Bitmap = new(
            "Bitmap", "image/bmp", new[] { "bmp" },
            prefixes: new[] { new Magic("BM") });

        public static readonly FileSignature Tiff = new(
            "TIFF", "image/tiff", new[] { "tif", "tiff" },
            prefixes: new[]
            {
                new Magic(Bytes(0x49, 0x49, 0x2A, 0x00)), // little endian
                new Magic(Bytes(0x4D, 0x4D, 0x00, 0x2A)), // big endian
            });

        public static readonly FileSignature RichText = new(
            "Rich Text", "application/rtf", new[] { "rtf" },
            prefixes: new[] { new Magic(@"{\rtf1") });

        // No magic - the extension is all there is. The deny list still gets a look at the bytes.
        public static readonly FileSignature Plaintext = new(
            "Plaintext", "text/plain", new[] { "txt" });

        // Text again: nothing to sniff, the extension stands alone and the deny list still reads the bytes.
        public static readonly FileSignature Xml = new(
            "XML", "application/xml", new[] { "xml" });

        public static readonly FileSignature Csv = new(
            "CSV", "text/csv", new[] { "csv" });

        // OOXML is a zip; the extension is what separates docx from xlsx from a plain archive.
        private static Magic[] ZipPrefixes => new[]
        {
            new Magic(Bytes(0x50, 0x4B, 0x03, 0x04)),
            new Magic(Bytes(0x50, 0x4B, 0x05, 0x06)), // empty archive
            new Magic(Bytes(0x50, 0x4B, 0x07, 0x08)), // spanned archive
        };

        // Compound File Binary - the container behind every Office 97-2003 document.
        private static Magic[] CompoundFilePrefixes => new[]
        {
            new Magic(Bytes(0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1)),
        };

        public static readonly FileSignature Word = new(
            "Word", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            new[] { "docx" }, prefixes: ZipPrefixes);

        public static readonly FileSignature Excel = new(
            "Excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            new[] { "xlsx" }, prefixes: ZipPrefixes);

        public static readonly FileSignature PowerPoint = new(
            "PowerPoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            new[] { "pptx" }, prefixes: ZipPrefixes);

        public static readonly FileSignature WordLegacy = new(
            "Word 97-2003", "application/msword", new[] { "doc" }, prefixes: CompoundFilePrefixes);

        public static readonly FileSignature ExcelLegacy = new(
            "Excel 97-2003", "application/vnd.ms-excel", new[] { "xls" }, prefixes: CompoundFilePrefixes);

        public static readonly FileSignature Zip = new(
            "Zip Archive", "application/zip", new[] { "zip" }, prefixes: ZipPrefixes);

        // --- forbidden -------------------------------------------------------------------

        public static readonly FileSignature Executable = new(
            "Executable", "application/octet-stream",
            new[] { "exe", "dll", "com", "scr", "msi", "cpl", "sys", "ocx", "drv" },
            prefixes: new[] { new Magic("MZ"), new Magic("ZM") });

        /// <summary>
        /// A PE hidden inside another file. Every PE carries the DOS stub's error string, so one
        /// floating match finds it wherever the header sits.
        /// ponytail: literal bytes only - a *compressed* executable inside an archive does not
        /// match. Unpack archives before checking if that matters.
        /// </summary>
        public static readonly FileSignature EmbeddedExecutable = new(
            "Embedded Executable", "application/octet-stream", extensions: null,
            anywhere: new[] { new Magic("This program cannot be run in DOS mode") });

        /// <summary>Scripts are plain text, so there is nothing to sniff - the extension is the rule.</summary>
        public static readonly FileSignature Script = new(
            "Script", "text/plain",
            new[]
            {
                "bat", "cmd", "ps1", "psm1", "vbs", "vbe", "js", "jse",
                "wsf", "wsh", "hta", "sh", "jar", "reg", "lnk", "msc",
            });

        public static readonly FileSignature Pdb = new(
            "Program Database", "application/octet-stream", new[] { "pdb" },
            prefixes: new[] { new Magic("Microsoft C/C++ ") });

        public static readonly FileSignature[] Forbidden =
        {
            Executable, EmbeddedExecutable, Script, Pdb,
        };

        /// <summary>Every type the catalog knows, minus the forbidden ones.</summary>
        public static readonly FileSignature[] All =
        {
            Pdf, Jpeg, Png, Gif, Bitmap, Tiff, RichText, Plaintext, Csv,
            Word, Excel, PowerPoint, WordLegacy, ExcelLegacy, Zip,
        };

        /// <summary>A checker permitting the named types and denying everything in <see cref="Forbidden"/>.</summary>
        public static FileTypeChecker Allowing(params FileSignature[] permitted) =>
            new FileTypeChecker().Permit(permitted).Deny(Forbidden);

        /// <summary>
        /// The catalog entries carrying any of the given MIME types. Lets an allow list be
        /// configured the way AppSettings already states it - as MIME types, not extensions.
        /// A MIME type the catalog does not know simply contributes nothing; the caller is
        /// expected to notice and complain, since silently allowing nothing is hard to debug.
        /// </summary>
        public static FileSignature[] WithMimeTypes(IEnumerable<string> mimeTypes)
        {
            var wanted = new HashSet<string>(mimeTypes ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            return All.Where(signature => wanted.Contains(signature.MimeType)).ToArray();
        }
    }
}
