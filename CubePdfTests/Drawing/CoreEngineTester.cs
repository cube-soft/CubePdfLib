﻿/* ------------------------------------------------------------------------- */
///
/// CoreEngineTester.cs
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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace CubePdfTests.Drawing
{
    /* --------------------------------------------------------------------- */
    ///
    /// CoreEngineTester
    ///
    /// <summary>
    /// CoreEngine クラスのテストをするためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    [TestFixture]
    public class CoreEngineTester
    {
        #region Setup and TearDown

        /* ----------------------------------------------------------------- */
        ///
        /// Setup
        /// 
        /// <summary>
        /// NOTE: テストに使用するサンプルファイル群は、テスト用プロジェクト
        /// フォルダ直下にある examples と言うフォルダに存在します。
        /// テストを実行する際には、実行ファイルをテスト用プロジェクトに
        /// コピーしてから行う必要があります（ビルド後イベントで、自動的に
        /// コピーされるように設定されてある）。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [SetUp]
        public void Setup()
        {
            var current = System.Environment.CurrentDirectory;
            _src = System.IO.Path.Combine(current, "Examples");
            _dest = System.IO.Path.Combine(current, "Results");
            if (!System.IO.Directory.Exists(_dest)) System.IO.Directory.CreateDirectory(_dest);
            _power = 0.1; // NOTE: イメージ作成のテスト時間を短縮するために生成倍率を下げる。
        }

        /* ----------------------------------------------------------------- */
        /// TearDown
        /* ----------------------------------------------------------------- */
        [TearDown]
        public void TearDown() { }

        #endregion

        #region Test Methods

        /* ----------------------------------------------------------------- */
        ///
        /// TestOpen
        /// 
        /// <summary>
        /// PDF ファイルを開くテストを行います。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestOpen()
        {
            // 1. ノーマルケース
            var filename = System.IO.Path.Combine(_src, "rotated.pdf");
            using (var engine = new CubePdf.Drawing.CoreEngine(filename))
            {
                Assert.Pass();
                Assert.AreEqual(9, engine.Pages.Count);
            }

            // 2. オーナパスワードを指定して開く
            filename = System.IO.Path.Combine(_src, "password.pdf");
            var password = "password"; // OwnerPassword
            using (var engine = new CubePdf.Drawing.CoreEngine(filename, password))
            {
                Assert.Pass();
                Assert.AreEqual(2, engine.Pages.Count);
            }

            // 3. ユーザパスワードを指定して開く
            password = "view"; // UserPassword
            using (var engine = new CubePdf.Drawing.CoreEngine(filename, password))
            {
                Assert.Pass();
                Assert.AreEqual(2, engine.Pages.Count);
            }

            // 4. 間違ったパスワードを指定する
            password = "invalid";
            try
            {
                using (var engine = new CubePdf.Drawing.CoreEngine(filename, password))
                {
                    Assert.Fail();
                }
            }
            catch (System.IO.FileLoadException /* e */)
            {
                Assert.Pass();
            }

            // 5. 存在しないファイルを指定する
            filename = "not-found.pdf";
            try
            {
                using (var engine = new CubePdf.Drawing.CoreEngine(filename))
                {
                    Assert.Fail();
                }
            }
            catch (System.IO.FileLoadException /* e */)
            {
                Assert.Pass();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestProperties
        /// 
        /// <summary>
        /// 各種プロパティをテストします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestProperties()
        {
            var src = System.IO.Path.Combine(_src, "rotated.pdf");
            Assert.IsTrue(System.IO.File.Exists(src));

            using (var engine = new CubePdf.Drawing.CoreEngine(src))
            {
                Assert.AreEqual(src, engine.FilePath);
                Assert.AreEqual(9, engine.Pages.Count);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestPageProperty
        /// 
        /// <summary>
        /// 各種ページの情報が取得できているかどうかテストします。
        /// テストに使用しているサンプルファイルは、全 9 ページの PDF で、
        /// 2 ページ目を 90 度、3ページ目を 180 度、4 ページ目を 270 度
        /// 回転させています。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestPages()
        {
            var src = System.IO.Path.Combine(_src, "rotated.pdf");
            Assert.IsTrue(System.IO.File.Exists(src));

            using (var engine = new CubePdf.Drawing.CoreEngine(src))
            {
                var page = engine.Pages[1];
                Assert.NotNull(page);
                Assert.AreEqual(engine.FilePath, page.FilePath);
                Assert.AreEqual(1, page.PageNumber);
                Assert.IsTrue(page.OriginalSize.Width > 0);
                Assert.IsTrue(page.OriginalSize.Height > 0);
                Assert.IsTrue(page.ViewSize.Width > 0);
                Assert.IsTrue(page.ViewSize.Height > 0);
                Assert.AreEqual(0, page.Rotation);
                Assert.AreEqual(1.0, page.Power);

                page = null;
                page = engine.Pages[2];
                Assert.NotNull(page);
                Assert.AreEqual(engine.FilePath, page.FilePath);
                Assert.AreEqual(2, page.PageNumber);
                Assert.IsTrue(page.OriginalSize.Width > 0);
                Assert.IsTrue(page.OriginalSize.Height > 0);
                Assert.IsTrue(page.ViewSize.Width > 0);
                Assert.IsTrue(page.ViewSize.Height > 0);
                Assert.AreEqual(90, page.Rotation);
                Assert.AreEqual(1.0, page.Power);

                page = null;
                page = engine.Pages[3];
                Assert.NotNull(page);
                Assert.AreEqual(engine.FilePath, page.FilePath);
                Assert.AreEqual(3, page.PageNumber);
                Assert.IsTrue(page.OriginalSize.Width > 0);
                Assert.IsTrue(page.OriginalSize.Height > 0);
                Assert.IsTrue(page.ViewSize.Width > 0);
                Assert.IsTrue(page.ViewSize.Height > 0);
                Assert.AreEqual(180, page.Rotation);
                Assert.AreEqual(1.0, page.Power);

                page = null;
                page = engine.Pages[4];
                Assert.NotNull(page);
                Assert.AreEqual(engine.FilePath, page.FilePath);
                Assert.AreEqual(4, page.PageNumber);
                Assert.IsTrue(page.OriginalSize.Width > 0);
                Assert.IsTrue(page.OriginalSize.Height > 0);
                Assert.IsTrue(page.ViewSize.Width > 0);
                Assert.IsTrue(page.ViewSize.Height > 0);
                Assert.AreEqual(270, page.Rotation);
                Assert.AreEqual(1.0, page.Power);

                page = null;
                page = engine.Pages[5];
                Assert.NotNull(page);
                Assert.AreEqual(engine.FilePath, page.FilePath);
                Assert.AreEqual(5, page.PageNumber);
                Assert.IsTrue(page.OriginalSize.Width > 0);
                Assert.IsTrue(page.OriginalSize.Height > 0);
                Assert.IsTrue(page.ViewSize.Width > 0);
                Assert.IsTrue(page.ViewSize.Height > 0);
                Assert.AreEqual(0, page.Rotation);
                Assert.AreEqual(1.0, page.Power);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestCreateImage
        /// 
        /// <summary>
        /// PDF ファイルの各ページのイメージを作成するテストです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestCreateImage()
        {
            var src = System.IO.Path.Combine(_src, "rotated.pdf");
            Assert.IsTrue(System.IO.File.Exists(src));

            using (var engine = new CubePdf.Drawing.CoreEngine(src))
            {
                foreach (var page in engine.Pages)
                {
                    Assert.AreEqual(page.Key, page.Value.PageNumber);
                    using (var image = engine.CreateImage(page.Key, _power))
                    {
                        Assert.NotNull(image);
                        Assert.AreEqual((int)(page.Value.ViewSize.Width * _power), image.Width);
                        Assert.AreEqual((int)(page.Value.ViewSize.Height * _power), image.Height);
                        var filename = String.Format("TestCreateImage-{0}.png", page.Key);
                        image.Save(System.IO.Path.Combine(_dest, filename));
                    }
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestCreateImageAsync
        /// 
        /// <summary>
        /// PDF ファイルの各ページのイメージを非同期で作成するテストです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestCreateImageAsync()
        {
            var src = System.IO.Path.Combine(_src, "rotated.pdf");
            Assert.IsTrue(System.IO.File.Exists(src));

            int created = 0;
            using (var engine = new CubePdf.Drawing.CoreEngine(src))
            {
                engine.ImageCreated += delegate(object sender, CubePdf.Drawing.ImageEventArgs e)
                {
                    Assert.NotNull(e.Image);
                    Assert.AreEqual(e.Page.ViewSize.Width, e.Image.Width);
                    Assert.AreEqual(e.Page.ViewSize.Height, e.Image.Height);
                    created += 1;
                    var filename = String.Format("TestCreateImageAsync-{0}.png", e.Page.PageNumber);
                    e.Image.Save(System.IO.Path.Combine(_dest, filename));
                    e.Image.Dispose();
                };

                for (int i = 0; i < engine.Pages.Count; ++i) engine.CreateImageAsync(i + 1, _power);
                while (engine.UnderImageCreation) System.Threading.Thread.Sleep(1);
                Assert.AreEqual(engine.Pages.Count, created);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TestCancelImageCreation
        /// 
        /// <summary>
        /// 非同期でイメージを作成中にキャンセルするテストです。
        /// 
        /// TODO: タイミングの問題か、savepoint と created の値が 1 ずれる
        /// 事がある。テスト方法（もしくは実装）を再考する。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        [Test]
        public void TestCancelImageCreation()
        {
            var src = System.IO.Path.Combine(_src, "rotated.pdf");
            Assert.IsTrue(System.IO.File.Exists(src));

            int created = 0;
            using (var engine = new CubePdf.Drawing.CoreEngine(src))
            {
                engine.ImageCreated += delegate(object sender, CubePdf.Drawing.ImageEventArgs e)
                {
                    Assert.NotNull(e.Image);
                    created += 1;
                    var filename = String.Format("TestCancelImageCreation-{0}.png", e.Page.PageNumber);
                    e.Image.Save(System.IO.Path.Combine(_dest, filename));
                    e.Image.Dispose();
                };

                for (int i = 0; i < engine.Pages.Count; ++i) engine.CreateImageAsync(i + 1, _power);
                engine.CancelImageCreation();
                var savepoint = created;
                while (engine.UnderImageCreation) System.Threading.Thread.Sleep(1);
                Assert.AreEqual(savepoint, created);
            }
        }

        #endregion

        #region Variables
        private string _src = string.Empty;
        private string _dest = string.Empty;
        private double _power = 1.0;
        #endregion
    }
}
