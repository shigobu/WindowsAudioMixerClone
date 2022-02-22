using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioMixerClone
{
    class Native
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint ExtractIconEx(string lpszFile, int nIconIndex,
        IntPtr[] phiconLarge, IntPtr phiconSmall, uint nIcons);

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint ExtractIconEx(string lpszFile, int nIconIndex,
            IntPtr phiconLarge, IntPtr phiconSmall, uint nIcons);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static public bool DestroyIcon(IntPtr handle);

        static public Icon[] ExtractIconEx(string fileName, int iconIndex, uint numIcon)
        {
            IntPtr[] hIcon = new IntPtr[numIcon];
            // アイコンハンドル＆ハンドル数の取得
            uint getCnt;
            getCnt = ExtractIconEx(fileName, iconIndex, hIcon, IntPtr.Zero, numIcon);
            if (getCnt < 1)
            {
                return new Icon[0];   // アイコンがなければ終了
            }
            if (getCnt == uint.MaxValue)
            {
                return new Icon[0];   // エラー
            }

            List<Icon> retVal = new List<Icon>();
            for (int idx = 0; idx < getCnt; idx++)
            {
                if (hIcon[idx] != IntPtr.Zero)
                {
                    retVal.Add(Icon.FromHandle(hIcon[idx]));
                }
            }
            return retVal.ToArray();
        }
    }
}