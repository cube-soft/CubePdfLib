﻿/* ------------------------------------------------------------------------- */
///
/// PdfReaderExtensions.cs
///
/// Copyright (c) 2010 CubeSoft, Inc. All rights reserved.
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
using iTextSharp.text.pdf;
using System.Drawing;
using CubePdf.Data;

namespace CubePdf.Editing.Extensions
{
    /* --------------------------------------------------------------------- */
    ///
    /// Cube.Pdf.Editing.Extensions.PdfReaderExtensions
    /// 
    /// <summary>
    /// iTextSharp の PdfReader に関する拡張メソッド群を定義するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal static class PdfReaderExtensions
    {
        /* ----------------------------------------------------------------- */
        ///
        /// CreatePage
        /// 
        /// <summary>
        /// Page オブジェクトを生成します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public static Page CreatePage(this PdfReader reader, string path, string password, int pagenum)
        {
            var size = reader.GetPageSize(pagenum);
            var dest = new Page();
            dest.FilePath = path;
            dest.OriginalSize = new Size((int)size.Width, (int)size.Height);
            dest.Rotation = reader.GetPageRotation(pagenum);
            dest.Password = password;
            dest.PageNumber = pagenum;
            return dest;
        }
    }
}

