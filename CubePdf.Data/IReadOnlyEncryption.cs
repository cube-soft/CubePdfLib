﻿/* ------------------------------------------------------------------------- */
///
/// IReadOnlyEncryption.cs
///
/// Copyright (c) 2013 CubeSoft, Inc. All rights reserved.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with this program.  If not, see < http://www.gnu.org/licenses/ >.
///
/* ------------------------------------------------------------------------- */
using System;

namespace CubePdf.Data
{
    /* --------------------------------------------------------------------- */
    ///
    /// IReadOnlyEncryption
    /// 
    /// <summary>
    /// PDF の暗号化に関するデータを読み取り専用で提供するための
    /// インターフェースです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public interface IReadOnlyEncryption
    {
        /* ----------------------------------------------------------------- */
        ///
        /// OwnerPassword
        /// 
        /// <summary>
        /// 所有者パスワードを取得します。所有者パスワードとは PDF ファイルに
        /// 設定されているマスターパスワードを表し、このパスワードによって
        /// 再暗号化や各種権限の変更等すべての操作が可能となります。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        string OwnerPassword { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// UserPassword
        /// 
        /// <summary>
        /// ユーザパスワードを取得します。ユーザパスワードとは PDF ファイルを
        /// 開く際に必要となるパスワードを表します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        string UserPassword { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Method
        /// 
        /// <summary>
        /// 適用する暗号化方式を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        EncryptionMethod Method { get; }

        /* ----------------------------------------------------------------- */
        ///
        /// Permission
        /// 
        /// <summary>
        /// 暗号化された PDF に設定されている各種権限の状態を取得します。
        /// 
        /// TODO: Permission プロパティの戻り値は IReadOnlyPermission と
        /// すべきだが（Permission オブジェクトのプロパティが変更可能に
        /// なってしまう）、Encryption クラスとの間で不整合が生じてしまう。
        /// 解決方法を検討する。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        Permission Permission { get; }
    }
}
