﻿/* ------------------------------------------------------------------------- */
///
/// ImagePage.cs
/// 
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Drawing;

namespace CubePdf.Data
{
    /* --------------------------------------------------------------------- */
    ///
    /// ImagePage
    /// 
    /// <summary>
    /// 単一イメージのみが存在する PDF のページを表すクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [Serializable]
    public class ImagePage : IPage
    {
        #region IPage properties

        /* ----------------------------------------------------------------- */
        ///
        /// Type
        /// 
        /// <summary>
        /// オブジェクトの種類を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public PageType Type
        {
            get { return PageType.Image; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Path
        /// 
        /// <summary>
        /// PDF ファイルのパスを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string FilePath { get; set; } = string.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// Size
        /// 
        /// <summary>
        /// 対象ページのオリジナルサイズを取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Size OriginalSize { get; set; } = Size.Empty;

        /* ----------------------------------------------------------------- */
        ///
        /// PageNumber
        /// 
        /// <summary>
        /// 該当ページのページ番号を取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public int PageNumber { get; set; } = 1;

        /* ----------------------------------------------------------------- */
        ///
        /// Rotation
        /// 
        /// <summary>
        /// 該当ページを表示する際の回転角を取得または設定します。
        /// </summary>
        /// 
        /// <remarks>
        /// 値は度単位 (degree) で設定して下さい。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public int Rotation { get; set; } = 0;

        /* ----------------------------------------------------------------- */
        ///
        /// Power
        /// 
        /// <summary>
        /// 該当ページの表示倍率を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public double Power { get; set; } = 1.0;

        #endregion

        #region IEquatable<IPage> methods

        /* ----------------------------------------------------------------- */
        ///
        /// Equals
        ///
        /// <summary>
        /// 引数に指定されたオブジェクトと等しいかどうか判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Equals(IPage obj)
        {
            var other = obj as ImagePage;
            if (other == null) return false;
            return FilePath == other.FilePath;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Equals
        ///
        /// <summary>
        /// 引数に指定されたオブジェクトと等しいかどうか判別します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;
            if (object.ReferenceEquals(this, obj)) return true;

            var other = obj as IPage;
            if (other == null) return false;

            return Equals(other);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// GetHashCode
        ///
        /// <summary>
        /// 特定の型のハッシュ関数として機能します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
