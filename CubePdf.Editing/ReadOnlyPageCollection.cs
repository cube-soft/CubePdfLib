﻿using System.Collections;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using CubePdf.Data;
using CubePdf.Editing.Extensions;

namespace CubePdf.Editing
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Pdf.Editing.ReadOnlyPageCollection
    /// 
    /// <summary>
    /// 読み取り専用で PDF ページ一覧へアクセスするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class ReadOnlyPageCollection : IReadOnlyCollection<IPage>
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ReadOnlyPageCollection
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ReadOnlyPageCollection() : this(null, string.Empty, string.Empty) { }

        /* ----------------------------------------------------------------- */
        ///
        /// ReadOnlyPageCollection
        /// 
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ReadOnlyPageCollection(PdfReader impl, string path, string password)
        {
            _impl = impl;
            _path = path;
            _password = password;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Count
        /// 
        /// <summary>
        /// ページ数を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int Count
        {
            get { return (_impl != null) ? _impl.NumberOfPages : 0; }
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// GetEnumerator
        /// 
        /// <summary>
        /// 各ページオブジェクトへアクセスするための反復子を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IEnumerator<IPage> GetEnumerator()
        {
            for (var i = 0; i < _impl.NumberOfPages; ++i)
            {
                var pagenum = i + 1;
                yield return _impl.CreatePage(_path, _password, pagenum);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetEnumerator
        /// 
        /// <summary>
        /// 各ページオブジェクトへアクセスするための反復子を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Fields
        private PdfReader _impl = null;
        private string _path = string.Empty;
        private string _password = string.Empty;
        #endregion
    }
}
