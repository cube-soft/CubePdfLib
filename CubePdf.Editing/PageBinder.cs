﻿/* ------------------------------------------------------------------------- */
///
/// DocumentReader.cs
///
/// Copyright (c) 2013 CubeSoft, Inc. All rights reserved.
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
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;

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
    public class PageBinder : CubePdf.Data.IDocumentWriter
    {
        #region Initialization and Termination

        /* ----------------------------------------------------------------- */
        ///
        /// PageBinder (constructor)
        /// 
        /// <summary>
        /// 既定の値で DocumentReader クラスを初期化します。
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
        /// PDF ファイルのメタデータを取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public CubePdf.Data.IMetadata Metadata
        {
            get { return _metadata; }
            set { _metadata = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Encryption
        /// 
        /// <summary>
        /// 暗号化に関する情報をを取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public CubePdf.Data.IEncryption Encryption
        {
            get { return _encrypt; }
            set { _encrypt = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Pages
        /// 
        /// <summary>
        /// PDF ファイルの各ページ情報を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ICollection<CubePdf.Data.IPage> Pages
        {
            get { return _pages; }
        }

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
        public bool UseSmartCopy
        {
            get { return _smart; }
            set { _smart = value; }
        }

        #endregion

        #region Public Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Reset
        /// 
        /// <summary>
        /// PageBinder クラスを初期状態にリセットします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Reset()
        {
            _metadata = new CubePdf.Data.Metadata();
            _encrypt = new CubePdf.Data.Encryption();
            _pages.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// 現在メンバ変数が保持している、メタデータ、暗号化に関する情報、
        /// 各ページ情報に基づいた PDF ファイルを指定されたパスに保存
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Save(string path)
        {
            try
            {
                var tmp = Path.GetTempFileName();
                File.Delete(tmp);
                BindPages(tmp);

                using (var reader = new PdfReader(tmp))
                using (var writer = new PdfStamper(reader, new FileStream(path, FileMode.Create)))
                {
                    AddMetadata(reader, writer);
                    AddSecurity(writer);
                    writer.SetFullCompression();
                    writer.Writer.Outlines = _bookmarks;
                }
                File.Delete(tmp);
            }
            catch (iTextSharp.text.pdf.BadPasswordException err)
            {
                throw new CubePdf.Data.EncryptionException(err.Message, err);
            }
        }

        #endregion

        #region Other methods

        /* ----------------------------------------------------------------- */
        ///
        /// BindPages
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
        private void BindPages(string dest)
        {
            var pagenum = 1;
            var readers = new Dictionary<string, iTextSharp.text.pdf.PdfReader>();
            var document = new iTextSharp.text.Document();
            var writer = _smart ? new PdfSmartCopy(document, new FileStream(dest, FileMode.Create)) :
                new PdfCopy(document, new FileStream(dest, FileMode.Create));            
            
            writer.PdfVersion = _metadata.Version.Minor.ToString()[0];            
            document.Open();
            _bookmarks.Clear();
            foreach (var page in _pages)
            {
                if (!readers.ContainsKey(page.FilePath))
                {
                    var item = page.Password.Length > 0 ?
                        new iTextSharp.text.pdf.PdfReader(page.FilePath, System.Text.Encoding.UTF8.GetBytes(page.Password)) :
                        new iTextSharp.text.pdf.PdfReader(page.FilePath);
                    readers.Add(page.FilePath, item);
                }

                var reader = readers[page.FilePath];
                var rot = reader.GetPageRotation(page.PageNumber);
                var dic = reader.GetPageN(page.PageNumber);
                if (rot != page.Rotation) dic.Put(PdfName.ROTATE, new PdfNumber(page.Rotation));

                writer.AddPage(writer.GetImportedPage(reader, page.PageNumber));
                AddBookmarks(reader, page.PageNumber, pagenum++);
            }
            document.Close();
            writer.Close();
            foreach (var reader in readers.Values) reader.Close();
            readers.Clear();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddBookmarks
        /// 
        /// <summary>
        /// PDF ファイルに存在するしおり情報を取得して追加します。
        /// </summary>
        /// 
        /// <remarks>
        /// 実際にしおりをPDFに追加するには PdfWriter クラスの Outlines
        /// プロパティに代入する必要があります。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private void AddBookmarks(PdfReader reader, int srcpage, int destpage)
        {
            var bookmarks = SimpleBookmark.GetBookmark(reader);
            if (bookmarks == null) return;

            var pattern = string.Format("^{0} (XYZ|Fit|FitH|FitBH)", destpage);
            SimpleBookmark.ShiftPageNumbers(bookmarks, destpage - srcpage, null);
            foreach (var bm in bookmarks)
            {
                if (bm.ContainsKey("Page") && Regex.IsMatch(bm["Page"].ToString(), pattern)) _bookmarks.Add(bm);
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
            info.Add("Title",    _metadata.Title);
            info.Add("Subject",  _metadata.Subtitle);
            info.Add("Keywords", _metadata.Keywords);
            info.Add("Creator",  _metadata.Creator);
            info.Add("Author",   _metadata.Author);
            writer.MoreInfo = info;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AddSecurity
        /// 
        /// <summary>
        /// 各種セキュリティ情報を付加します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void AddSecurity(PdfStamper writer)
        {
            if (_encrypt.IsEnabled && _encrypt.OwnerPassword.Length > 0)
            {
                var method = Translator.ToIText(_encrypt.Method);
                var permission = Translator.ToIText(_encrypt.Permission);
                var userpassword = _encrypt.IsUserPasswordEnabled ? _encrypt.UserPassword : "";
                writer.Writer.SetEncryption(method, userpassword, _encrypt.OwnerPassword, permission);
            }
        }

        #endregion

        #region Variables
        private CubePdf.Data.IMetadata _metadata = new CubePdf.Data.Metadata();
        private CubePdf.Data.IEncryption _encrypt = new CubePdf.Data.Encryption();
        private IList<CubePdf.Data.IPage> _pages = new List<CubePdf.Data.IPage>();
        private bool _smart = true;
        private IList<Dictionary<String, Object>> _bookmarks = new List<Dictionary<String, Object>>();
        #endregion
    }
}
