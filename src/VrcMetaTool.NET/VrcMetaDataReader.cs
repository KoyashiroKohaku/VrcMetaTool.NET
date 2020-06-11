using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KoyashiroKohaku.PngChunkUtil;
using KoyashiroKohaku.VrcMetaTool.Properties;

namespace KoyashiroKohaku.VrcMetaTool
{
    /// <summary>
    /// VrcMetaDataReader
    /// </summary>
    public static class VrcMetaDataReader
    {
        /// <summary>
        /// バイト配列がPNG画像のとき<see cref="true"/>を返します。
        /// </summary>
        /// <param name="buffer">バイト配列</param>
        /// <returns>バイト配列がPNG画像のとき<see cref="true"/>, それ以外のとき<see cref="false"/></returns>
        public static bool IsPng(ReadOnlySpan<byte> buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return ChunkReader.IsPng(buffer);
        }

        /// <summary>
        /// PNG画像のバイト配列からmeta情報を抽出します。
        /// </summary>
        /// <param name="buffer">バイト配列</param>
        /// <returns>meta情報</returns>
        public static VrcMetaData Read(ReadOnlySpan<byte> buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (!buffer[..8].SequenceEqual(PngUtil.Signature))
            {
                throw new ArgumentException(Resources.VrcMetaReader_Read_ArgumentException, nameof(buffer));
            }

            var chunks = ChunkReader.GetChunks(buffer, ChunkTypeFilter.AdditionalChunkOnly);

            var vrcMetaData = new VrcMetaData();

            var dateChunk = chunks.FirstOrDefault(c => c.TypePart.SequenceEqual(VrcMetaChunk.DateChunk));
            if (dateChunk != null)
            {
                if (DateTime.TryParseExact(dateChunk.DataString, "yyyyMMddHHmmssfff", new CultureInfo("en", false), DateTimeStyles.None, out var parsedDate))
                {
                    vrcMetaData.Date = parsedDate;
                }
                else
                {
                    vrcMetaData.Date = null;
                }
            }
            else
            {
                vrcMetaData.Date = null;
            }

            var worldChunk = chunks.FirstOrDefault(c => c.TypePart.SequenceEqual(VrcMetaChunk.WorldChunk));
            vrcMetaData.World = worldChunk?.DataString;

            var photographerChunk = chunks.FirstOrDefault(c => c.TypePart.SequenceEqual(VrcMetaChunk.PhotographerChunk));
            vrcMetaData.Photographer = photographerChunk?.DataString;

            var users = chunks.Where(c => c.TypePart.SequenceEqual(VrcMetaChunk.UserChunk));
            vrcMetaData.Users.AddRange(chunks.Select(c => new User(c.DataString)));

            return vrcMetaData;
        }

        /// <summary>
        /// ファイルパスからPNG画像を読み込みmeta情報を抽出します。
        /// </summary>
        /// <param name="path">PNG画像のファイルパス</param>
        /// <returns>meta情報</returns>
        public static VrcMetaData Read(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            return Read(File.ReadAllBytes(path));
        }

        /// <summary>
        /// ファイルパスからPNG画像を読み込みmeta情報を抽出します。
        /// </summary>
        /// <param name="path">PNG画像のファイルパス</param>
        /// <returns>meta情報</returns>
        public static async Task<VrcMetaData> ReadAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            return await ReadAsync(await File.ReadAllBytesAsync(path).ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// PNG画像のバイナリデータからmeta情報を抽出します。
        /// </summary>
        /// <param name="buffer">PNG画像のバイナリ</param>
        /// <returns>meta情報</returns>
        public static Task<VrcMetaData> ReadAsync(byte[] buffer) => Task.Run(() => Read(buffer));
    }
}