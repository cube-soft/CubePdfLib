﻿/* ------------------------------------------------------------------------- */
///
/// PageBinder.cs
///
/// Copyright (c) 2010 CubeSoft, Inc.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as published
/// by the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.exceptions;
using CubePdf.Editing.Extensions;
using CubePdf.Data;
using IoEx = System.IO;

namespace CubePdf.Editing
{
    /* --------------------------------------------------------------------- */
    ///
    /// PageBinder
    /// 
    /// <summary>
    /// 複数の PDF ファイルのページの一部、または全部をまとめて一つの
    /// PDF ファイルにするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class PageBinder : IDocumentWriter
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// PageBinder
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public PageBinder() { }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Metadata
        /// 
        /// <summary>
        /// PDF ファイルのメタデータを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Metadata Metadata { get; set; } = new Metadata();

        /* ----------------------------------------------------------------- */
        ///
        /// Encryption
        /// 
        /// <summary>
        /// 暗号化に関する情報をを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Encryption Encryption { get; set; } = new Encryption();

        /* ----------------------------------------------------------------- */
        ///
        /// Pages
        /// 
        /// <summary>
        /// PDF ファイルの各ページ情報を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICollection<IPage> Pages { get; } = new List<IPage>();

        /* ----------------------------------------------------------------- */
        ///
        /// UseSmartCopy
        /// 
        /// <summary>
        /// ファイルサイズを抑えるための結合方法を使用するかどうかを取得、
        /// または設定します。
        /// </summary>
        /// 
        /// <remarks>
        /// 通常時には iTextSharp の PdfCopy クラスを用いて結合を行って
        /// いるが、このクラスは複数の PDF ファイルが同じフォントを使用
        /// していたとしても別々のものとして扱うため、フォント情報が重複して
        /// ファイルサイズが増大する場合がある。
        /// 
        /// この問題を解決したものとして PdfSmartCopy クラスが存在する。
        /// ただし、複雑な注釈が保存されている PDF を結合する際に使用した
        /// 場合、（別々として扱わなければならないはずの）情報が共有されて
        /// しまい、注釈の構造が壊れてしまう問題が確認されている。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public bool UseSmartCopy { get; set; } = true;

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        /// 
        /// <summary>
        /// 初期状態にリセットします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Reset()
        {
            Metadata = new Metadata();
            Encryption = new Encryption();
            Pages.Clear();
        }

        #endregion

        #region Other private methods

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// メンバ変数が保持している、メタデータ、暗号化に関する情報、
        /// 各ページ情報に基づいた PDF ファイルを指定されたパスに保存
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Save(string path)
        {
            var tmp = IoEx.Path.GetTempFileName();

            try
            {
                Bind(tmp);
                using (var reader = new PdfReader(tmp))
                using (var writer = new PdfStamper(reader, new IoEx.FileStream(path, IoEx.FileMode.Create)))
                {
                    AddMetadata(reader, writer);
                    AddEncryption(writer);
                    if (Metadata.Version.Minor >= 5) writer.SetFullCompression();
                    writer.Writer.Outlines = _bookmarks;
                }
            }
            catch (BadPasswordException err) { throw new EncryptionException(err.Message, err); }
            finally { TryDelete(tmp); }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Bind
        /// 
        /// <summary>
        /// 指定された各ページを結合し、新たな PDF ファイルを生成します。
        /// </summary>
        /// 
        /// <remarks>
        /// 注釈等を含めて完全にページ内容をコピーするためにいったん
        /// PdfCopy クラスを用いて全ページを結合します。
        /// セキュリティ設定や文書プロパティ等の情報は生成された PDF に
        /// 対して付加します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void Bind(string dest)
        {
            if (IoEx.File.Exists(dest)) IoEx.File.Delete(dest);

            var readers = new Dictionary<string, PdfReader>();
            var document = new iTextSharp.text.Document();
            var writer = UseSmartCopy ?
                           new PdfSmartCopy(document, new IoEx.FileStream(dest, IoEx.FileMode.Create)) :
                           new PdfCopy(document, new IoEx.FileStream(dest, IoEx.FileMode.Create));

            writer.PdfVersion = Metadata.Version.Minor.ToString()[0];
            writer.ViewerPreferences = Metadata.ViewerPreferences;

            document.Open();
            _bookmarks.Clear();
            foreach (var page in Pages)
            {
                switch (page.Type)
                {
                    case PageType.Image:
                        AddImagePage(page as ImagePage, writer);
                        break;
                    case PageType.Pdf:
                        AddPage(page as Page, writer, readers);
                        break;
                    default:
                        break;
                }
            }

            document.Close();
            writer.Close();
            foreach (var reader in readers.Values) reader.Close();
            readers.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddPage
        /// 
        /// <summary>
        /// PDF ページを追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddPage(PdfReader src, PdfCopy dest)
        {
            for (var i = 0; i < src.NumberOfPages; ++i)
            {
                var page = dest.GetImportedPage(src, i + 1);
                dest.AddPage(page);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddPage
        /// 
        /// <summary>
        /// PDF ページを追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddPage(Page src, PdfCopy dest, Dictionary<string, PdfReader> readers)
        {
            if (src == null) return;

            if (!readers.ContainsKey(src.FilePath))
            {
                var item = src.Password.Length > 0 ?
                           new PdfReader(src.FilePath, System.Text.Encoding.UTF8.GetBytes(src.Password)) :
                           new PdfReader(src.FilePath);
                readers.Add(src.FilePath, item);
            }

            var reader = readers[src.FilePath];
            var rot = reader.GetPageRotation(src.PageNumber);
            var dic = reader.GetPageN(src.PageNumber);
            if (rot != src.Rotation) dic.Put(PdfName.ROTATE, new PdfNumber(src.Rotation));

            dest.AddPage(dest.GetImportedPage(reader, src.PageNumber));
            StockBookmarks(reader, src.PageNumber, dest.PageNumber);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddImagePage
        /// 
        /// <summary>
        /// 画像ファイルを 1 ページの PDF として追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddImagePage(ImagePage src, PdfCopy dest)
        {
            if (src == null) return;

            using (var image = new System.Drawing.Bitmap(src.FilePath))
            using (var stream = new IoEx.MemoryStream())
            {
                var document = new iTextSharp.text.Document();
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                var guid = image.FrameDimensionsList[0];
                var dimension = new System.Drawing.Imaging.FrameDimension(guid);
                for (var i = 0; i < image.GetFrameCount(dimension); ++i)
                {
                    image.SelectActiveFrame(dimension, i);

                    var dpi = GetImageDpiScale(image);
                    var width = src.OriginalSize.IsEmpty ? (int)(image.Width * dpi) : src.OriginalSize.Width;
                    var height = src.OriginalSize.IsEmpty ? (int)(image.Height * dpi) : src.OriginalSize.Height;
                    var obj = CreateImage(src, image);

                    document.SetPageSize(new iTextSharp.text.Rectangle(width, height));
                    document.NewPage();
                    document.Add(obj);
                }

                document.Close();
                writer.Close();

                using (var reader = new PdfReader(stream.ToArray())) AddPage(reader, dest);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddMetadata
        /// 
        /// <summary>
        /// タイトル、著者名等の各種メタデータを追加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddMetadata(PdfReader reader, PdfStamper writer)
        {
            var info = reader.Info;
            info.Add("Title", Metadata.Title);
            info.Add("Subject", Metadata.Subtitle);
            info.Add("Keywords", Metadata.Keywords);
            info.Add("Creator", Metadata.Creator);
            info.Add("Author", Metadata.Author);
            writer.MoreInfo = info;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddEncryption
        /// 
        /// <summary>
        /// 各種セキュリティ情報を付加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddEncryption(PdfStamper writer)
        {
            if (Encryption.IsEnabled && Encryption.OwnerPassword.Length > 0)
            {
                var method = Translator.ToIText(Encryption.Method);
                var permission = Translator.ToIText(Encryption.Permission);
                var userpass = Encryption.IsUserPasswordEnabled ?
                                 GetUserPassword(Encryption.UserPassword, Encryption.OwnerPassword) :
                                 string.Empty;
                writer.Writer.SetEncryption(method, userpass, Encryption.OwnerPassword, permission);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// StockBookmarks
        /// 
        /// <summary>
        /// PDF ファイルに存在するしおり情報を取得して保存しておきます。
        /// </summary>
        /// 
        /// <remarks>
        /// 実際にしおりを PDF に追加するには PdfWriter クラスの Outlines
        /// プロパティに代入する必要があります。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void StockBookmarks(PdfReader reader, int srcPage, int destPage)
        {
            var bookmarks = SimpleBookmark.GetBookmark(reader);
            if (bookmarks == null) return;

            var pattern = string.Format("^{0} (XYZ|Fit|FitH|FitBH)", destPage);
            SimpleBookmark.ShiftPageNumbers(bookmarks, destPage - srcPage, null);
            foreach (var bm in bookmarks)
            {
                if (bm.ContainsKey("Page") && Regex.IsMatch(bm["Page"].ToString(), pattern)) _bookmarks.Add(bm);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateImage
        /// 
        /// <summary>
        /// イメージオブジェクトを生成します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private iTextSharp.text.Image CreateImage(ImagePage src, System.Drawing.Image image)
        {
            var width = src.OriginalSize.IsEmpty ? image.Width : src.OriginalSize.Width;
            var height = src.OriginalSize.IsEmpty ? image.Height : src.OriginalSize.Height;
            var dpi = GetImageDpiScale(image);
            var scale = GetImageScale(image, width, height);
            var pos = src.OriginalSize.IsEmpty ?
                         new System.Drawing.Point(0, 0) :
                         GetImagePosition(image, width, height, scale * dpi);

            var dest = iTextSharp.text.Image.GetInstance(image, image.GuessImageFormat());
            dest.SetAbsolutePosition(pos.X, pos.Y);
            dest.ScalePercent((float)(scale * dpi * 100.0));
            return dest;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetImageDpiScale
        /// 
        /// <summary>
        /// イメージの解像度の差を考慮した縮小倍率を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private double GetImageDpiScale(System.Drawing.Image image)
        {
            var w = 72.0 / image.HorizontalResolution;
            var h = 72.0 / image.VerticalResolution;
            return Math.Min(w, h);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetImageScale
        /// 
        /// <summary>
        /// イメージの縮小倍率を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private double GetImageScale(System.Drawing.Image image, int width, int height)
        {
            var x = width / (double)image.Width;
            var y = height / (double)image.Height;
            return Math.Min(Math.Min(x, y), 1.0);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetImagePosition
        /// 
        /// <summary>
        /// イメージの表示位置を取得します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private System.Drawing.Point GetImagePosition(System.Drawing.Image image, int width, int height, double ratio)
        {
            var x = (width - image.Width * ratio) / 2.0;
            var y = (height - image.Height * ratio) / 2.0;
            return new System.Drawing.Point((int)x, (int)y);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetUserPassword
        /// 
        /// <summary>
        /// ユーザパスワードを取得します。
        /// </summary>
        /// 
        /// <remarks>
        /// ユーザから明示的にユーザパスワードが指定されていない場合、
        /// オーナパスワードと同じ文字列を使用します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private string GetUserPassword(string userPassword, string ownerPassword)
        {
            return !string.IsNullOrEmpty(userPassword) ? userPassword : ownerPassword;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TryDelete
        /// 
        /// <summary>
        /// ファイルの削除を試行します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private bool TryDelete(string path)
        {
            try
            {
                IoEx.File.Delete(path);
                return true;
            }
            catch (Exception /* err */) { return false; }
        }

        #endregion

        #region Fields
        private List<Dictionary<string, object>> _bookmarks = new List<Dictionary<string, object>>();
        #endregion
    }
}
